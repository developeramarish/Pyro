﻿using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Pyro.Common.CompositionRoot;
using Pyro.Common.Exceptions;
using Pyro.Common.FhirOperation.BundleTransaction;
using Pyro.Common.Interfaces.Repositories;
using Pyro.Common.Interfaces.Service;
using Pyro.Common.Logging;
using Pyro.Common.Service.ResourceService;
using Pyro.Common.Service.Trigger;
using Pyro.Common.Tools.FhirTask;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Web.Hosting;

namespace Pyro.Engine.Services.FhirTasks.FhirSpecLoader
{
  public class FhirSpecificationDefinitionLoader : IFhirSpecificationDefinitionLoader
  {
    private readonly ILog ILog;
    private readonly IUnitOfWork IUnitOfWork;
    private readonly IRequestMetaFactory IRequestMetaFactory;
    private readonly IResourceTriggerService IResourceTriggerService;
    private readonly IBundleTransactionOperationFactory IBundleTransactionOperationFactory;
    private readonly IFhirSpecificationDefinitionLoaderParameters IFhirSpecificationDefinitionLoaderParameters;
    private readonly IFhirTaskTool IFhirTaskTool;
    private readonly Common.PyroHealthInformation.CodeSystems.IPyroFhirServer IPyroFhirServerCodeSystem;

    private readonly string _ZipFileName = "definitions.xml.zip";
    private readonly string _TaskStatusSystem = "http://hl7.org/fhir/task-status";

    public FhirSpecificationDefinitionLoader(ILog ILog, 
      IUnitOfWork IUnitOfWork, 
      IRequestMetaFactory IRequestMetaFactory, 
      IResourceTriggerService IResourceTriggerService, 
      IBundleTransactionOperationFactory IBundleTransactionOperationFactory, 
      IFhirSpecificationDefinitionLoaderParameters IFhirSpecificationDefinitionLoaderParameters, 
      IFhirTaskTool IFhirTaskTool,
      Common.PyroHealthInformation.CodeSystems.IPyroFhirServer IPyroFhirServerCodeSystem)
    {
      this.ILog = ILog;
      this.IUnitOfWork = IUnitOfWork;
      this.IRequestMetaFactory = IRequestMetaFactory;
      this.IResourceTriggerService = IResourceTriggerService;
      this.IBundleTransactionOperationFactory = IBundleTransactionOperationFactory;
      this.IFhirSpecificationDefinitionLoaderParameters = IFhirSpecificationDefinitionLoaderParameters;
      this.IFhirTaskTool = IFhirTaskTool;
      this.IPyroFhirServerCodeSystem = IPyroFhirServerCodeSystem;
    }

    //Task.Status (Draft -> Requested -> Received -> Accepted -> Rejected ->
    //             Ready -> Cancelled -> In Progress -> On Hold -> 
    //             Failed -> Completed -> Entered in Error)

    public Task.TaskStatus Run(Task Task)
    {
      try
      {
        //Task.TaskStatus? LocalTaskStatus = null;
        Task.ExecutionPeriod = new Period();
        Task.ExecutionPeriod.StartElement = new FhirDateTime(DateTimeOffset.Now);
        SetParametersBeforeRunningTaskLoad(Task);
        Task.Output = new List<Task.OutputComponent>();
        //We process each file in the zip one at a time and commmit and update the Task each time.
        //Once the IFhirSpecificationDefinitionLoaderParameters.TaskStatus == Completed we then return, or return is the Load retunes false. 
        while (IFhirSpecificationDefinitionLoaderParameters.TaskStatus == Task.TaskStatus.InProgress)
        {
          bool AllOk = false;
          using (DbContextTransaction Transaction = IUnitOfWork.BeginTransaction())
          {
            AllOk = LoadFromZip(IFhirSpecificationDefinitionLoaderParameters);
            if (AllOk)
            {
              SetParametersInProgressTaskLoad(Task);
              IFhirTaskTool.UpdateTaskAsStatus(IFhirSpecificationDefinitionLoaderParameters.TaskStatus.Value, Task);
              Transaction.Commit();
            }
            else
            {
              Transaction.Rollback();
              SetParametersOnFailedTaskLoad(Task);
            }
          }

          //Update the Task as Failed in new Transaction
          if (!AllOk)
            using (DbContextTransaction Transaction = IUnitOfWork.BeginTransaction())
            {
              IFhirTaskTool.UpdateTaskAsStatus(IFhirSpecificationDefinitionLoaderParameters.TaskStatus.Value, Task);
              Transaction.Commit();
              return IFhirSpecificationDefinitionLoaderParameters.TaskStatus.Value;
            }
        }

        SetParametersOnCompletedTaskLoad(Task);
        return IFhirSpecificationDefinitionLoaderParameters.TaskStatus.Value;
      }
      catch (Exception Exec)
      {
        ILog.Error(Exec, $"Internal Server Error: Transaction was rolled back. " +
          $"Task reference was: {Task.ResourceType.GetLiteral()}/{Task.Id}/_history/{Task.VersionId} The Task was not processed.");
        return Task.TaskStatus.Failed;
      }
    }

    private bool LoadFromZip(IFhirSpecificationDefinitionLoaderParameters Parameters)
    {
      bool OneFileCompleted = false;
      string ZipFilePath = GetZipFilePath();
      Stream FileStream = null;
      try
      {
        FileStream = new FileStream(ZipFilePath, FileMode.Open, FileAccess.Read);
      }
      catch (Exception Exec)
      {
        string ErrorMessage = $"Internal Server Error: Could not read the FHIR specification zip file named: {_ZipFileName} from the server path {ZipFilePath}.";
        Parameters.ErrorMessage = ErrorMessage;
        Parameters.TaskStatus = Hl7.Fhir.Model.Task.TaskStatus.Failed;
        ILog.Error(Exec, ErrorMessage);
        return false;
      }

      try
      {
        using (ZipArchive Archive = new ZipArchive(FileStream))
        {
          foreach (ZipArchiveEntry Entry in Archive.Entries)
          {
            if (Entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
              //Pick processing from where it was last left off
              if (!Parameters.FileCompletedList.Contains(Entry.FullName) || Parameters.FileCompletedList.Count == 0)
              {
                Parameters.FileInProgress = Entry.FullName;
                if (OneFileCompleted)
                {
                  //We only want to do one file in the ZIP package at a time but we want the name of the next file to process
                  //This is why this is here.
                  //Yes I know we are unzipping the file many times but this is better that staring all over again each time we don not get to finish the entire set.
                  Parameters.FileInProgress = Entry.FullName;
                  return true;
                }

                Stream StreamItem = Entry.Open();
                using (StreamItem)
                {
                  try
                  {
                    var buffer = new MemoryStream();
                    StreamItem.CopyTo(buffer);
                    buffer.Seek(0, SeekOrigin.Begin);
                    System.Xml.XmlReader XmlReader = SerializationUtil.XmlReaderFromStream(buffer);
                    Resource Resource = Common.Tools.FhirResourceSerializationSupport.DeSerializeFromXml(XmlReader);
                    if (Resource is Bundle Bundle)
                    {
                      if (ConvertToTransactionBundle(Bundle, Entry.FullName))
                      {
                        if (CommitTransactionBundle(Bundle, Entry.FullName))
                        {
                          Parameters.FileCompletedList.Add(Entry.FullName);
                          Parameters.TaskStatus = Hl7.Fhir.Model.Task.TaskStatus.InProgress;
                          OneFileCompleted = true; ;
                        }
                        else
                        {
                          string ErrorMessage = $"Internal Server Error: FHIR specification resource bundle named {Entry.FullName} from the FHIR specification zip file named: {_ZipFileName} was unable to be commited as a transaction bundle. See server logs for more info.";
                          ILog.Error(ErrorMessage);
                          Parameters.FileInError = Entry.FullName;
                          Parameters.ErrorMessage = ErrorMessage;
                          return false;
                        }
                      }
                      else
                      {
                        string ErrorMessage = $"Internal Server Error: Unable to convert the FHIR specification resource bundle named {Entry.FullName} from the FHIR specification zip file named: {_ZipFileName}. See server logs for more info.";
                        ILog.Error(ErrorMessage);
                        Parameters.FileInError = Entry.FullName;
                        Parameters.ErrorMessage = ErrorMessage;
                        return false;
                      }
                    }
                  }
                  catch (PyroException PyroExec)
                  {
                    string ErrorMessage = $"Internal Server Error: Exception thrown when DeSerializing FHIR resources from the FHIR specification zip file named: {_ZipFileName}. See server logs for more info.";
                    ILog.Error(PyroExec, ErrorMessage);
                    Parameters.FileInError = Entry.FullName;
                    Parameters.ErrorMessage = ErrorMessage;
                    return false;
                  }
                }

              }
            }
          }
        }
      }
      catch (Exception Exec)
      {
        string ErrorMessage = $"Internal Server Error: Exception thrown when unzipping FHIR specification zip file named: {_ZipFileName}.";
        Parameters.ErrorMessage = ErrorMessage;
        Parameters.TaskStatus = Hl7.Fhir.Model.Task.TaskStatus.Failed;
        ILog.Error(Exec, ErrorMessage);
        return false;
      }
      //no errors after all processed so return true.
      //Completed to end so reset all parameters for return.
      Parameters.FileInProgress = string.Empty;
      Parameters.TaskStatus = Hl7.Fhir.Model.Task.TaskStatus.Completed;
      Parameters.ErrorMessage = string.Empty;
      Parameters.FileInError = string.Empty;
      return true;
    }

    private bool ConvertToTransactionBundle(Bundle Bundle, string FileName)
    {
      Bundle.Type = Bundle.BundleType.Transaction;
      foreach (var Entry in Bundle.Entry)
      {
        if (Entry.Resource != null)
        {
          //Add Protected to all resource from the FHIR Spec
          IPyroFhirServerCodeSystem.SetProtectedMetaTag(Entry.Resource);
          if (!string.IsNullOrWhiteSpace(Entry.Resource.Id))
          {
            Entry.Request = new Bundle.RequestComponent();
            Entry.Request.Method = Bundle.HTTPVerb.PUT;
            Entry.Request.Url = $"{Entry.Resource.ResourceType.GetLiteral()}/{Entry.Resource.Id}";
          }
          else
          {
            ILog.Warn($"Internal Server Error: A bundle entry in the file named {FileName} in the FHIR specification zip file name {_ZipFileName}, had no resource Id.");
            return false;
          }
        }
        else
        {
          ILog.Warn($"Internal Server Error: A bundle entry in the file named {FileName} in the FHIR specification zip file name {_ZipFileName}, had a null resource.");
          return false;
        }
      }
      return true;
    }

    private bool CommitTransactionBundle(Bundle Bundle, string FileName)
    {
      try
      {
        var RequestMeta = IRequestMetaFactory.CreateRequestMeta().Set($"");
        IBundleTransactionOperation BundleTransactionOperation = IBundleTransactionOperationFactory.CreateBundleTransactionOperation();
        this.IResourceTriggerService.TriggersActive = false;
        IResourceServiceOutcome ResourceServiceOutcome = BundleTransactionOperation.Transact(Bundle, RequestMeta);
        this.IResourceTriggerService.TriggersActive = true;
        if (ResourceServiceOutcome.SuccessfulTransaction)
        {
          return true;
        }
        else
        {
          string OperationOutcomeMessage = string.Empty;
          if (ResourceServiceOutcome.ResourceResult != null && ResourceServiceOutcome.ResourceResult is OperationOutcome OptOut)
          {
            OptOut.Issue.ForEach(x => OperationOutcomeMessage = OperationOutcomeMessage + " " + x.Details);
          }
          ILog.Error($"Internal Server Error: Transaction bundle from the FHIR specification zip file named: {_ZipFileName}, file '{FileName} did not commit in a single transaction. " +
            $"The POST bundle transaction reported: HTTP Status: {ResourceServiceOutcome.HttpStatusCode} and the detail: {OperationOutcomeMessage}");
          return false;
        }
      }
      catch (Exception Exec)
      {
        ILog.Error(Exec, $"Internal Server Error: Could not commit the transaction bundle from the FHIR specification zip file named: {_ZipFileName}, file '{FileName}.");
        return false;
      }

    }

    private void SetParametersBeforeRunningTaskLoad(Task Task)
    {
      IFhirSpecificationDefinitionLoaderParameters.TaskStatus = Task.Status;
      IFhirSpecificationDefinitionLoaderParameters.FileCompletedList = new List<string>();


      foreach (var Out in Task.Output)
      {
        if (Out.Type.Coding.Any(x => x.System == _TaskStatusSystem && x.Code == Task.TaskStatus.Completed.GetLiteral()))
        {
          if (Out.Value is FhirString FileNameString)
          {
            IFhirSpecificationDefinitionLoaderParameters.FileCompletedList.Add(FileNameString.Value);
          }
        }
        else if (Out.Type.Coding.Any(x => x.System == _TaskStatusSystem && x.Code == Task.TaskStatus.InProgress.GetLiteral()))
        {
          if (string.IsNullOrWhiteSpace(IFhirSpecificationDefinitionLoaderParameters.FileInProgress))
          {
            if (Out.Value is FhirString FileNameString)
            {
              IFhirSpecificationDefinitionLoaderParameters.FileInProgress = FileNameString.Value;
            }
          }
          else
          {
            throw new Exception($"Internal Server Error: Task can not have more than one FIle in Progress at any one time. Task is {Task.ResourceType.GetLiteral()}/{Task.Id}");
          }
        }
        //Note: we ignore any FileInError as we only collect Task of status == InProgress or Ready.
        // If a user somehow changed a Task status back to InProgress or Ready and yet an old Output is set to Failed the we
        // are to asume they want us to try again, in this case the FileInError and FileInProgress should be the same file.
        // The process will start again at the FileInProgress.
      }
    }

    private void SetParametersInProgressTaskLoad(Task Task)
    {
      Task.Status = IFhirSpecificationDefinitionLoaderParameters.TaskStatus.Value;

      Task.Output = new List<Task.OutputComponent>();
      //Update Task Compeleted LIst
      foreach (string FileNameCompleted in IFhirSpecificationDefinitionLoaderParameters.FileCompletedList)
      {
        var FhirStringValue = new FhirString(FileNameCompleted);
        var MyCodableConcept = new CodeableConcept(_TaskStatusSystem, Task.TaskStatus.Completed.GetLiteral(), Task.TaskStatus.Completed.GetLiteral());
        Task.Output.Add(new Task.OutputComponent() { Type = MyCodableConcept, Value = FhirStringValue });
      }

      if (!string.IsNullOrWhiteSpace(IFhirSpecificationDefinitionLoaderParameters.FileInProgress))
      {
        var FhirStringValue = new FhirString(IFhirSpecificationDefinitionLoaderParameters.FileInProgress);
        var MyCodableConcept = new CodeableConcept(_TaskStatusSystem, Task.TaskStatus.InProgress.GetLiteral(), Task.TaskStatus.InProgress.GetLiteral());
        Task.Output.Add(new Task.OutputComponent() { Type = MyCodableConcept, Value = FhirStringValue });
      }
    }

    private void SetParametersOnCompletedTaskLoad(Task Task)
    {
      Task.Status = IFhirSpecificationDefinitionLoaderParameters.TaskStatus.Value;
      Task.ExecutionPeriod.EndElement = new FhirDateTime(DateTimeOffset.Now);

      Task.Output = new List<Task.OutputComponent>();
      //Update Task Compeleted LIst
      foreach (string FileNameCompleted in IFhirSpecificationDefinitionLoaderParameters.FileCompletedList)
      {
        var FhirStringValue = new FhirString(FileNameCompleted);
        var MyCodableConcept = new CodeableConcept(_TaskStatusSystem, Task.TaskStatus.Completed.GetLiteral(), Task.TaskStatus.Completed.GetLiteral());
        Task.Output.Add(new Task.OutputComponent() { Type = MyCodableConcept, Value = FhirStringValue });
      }
    }
    
    private void SetParametersOnFailedTaskLoad(Task Task)
    {
      Task.Status = IFhirSpecificationDefinitionLoaderParameters.TaskStatus.Value;
      Task.ExecutionPeriod.EndElement = new FhirDateTime(DateTimeOffset.Now);

      Task.Output = new List<Task.OutputComponent>();
      //Update Task Compeleted LIst
      foreach (string FileNameCompleted in IFhirSpecificationDefinitionLoaderParameters.FileCompletedList)
      {
        var FhirStringValue = new FhirString(FileNameCompleted);
        var MyCodableConcept = new CodeableConcept(_TaskStatusSystem, Task.TaskStatus.Completed.GetLiteral(), Task.TaskStatus.Completed.GetLiteral());
        Task.Output.Add(new Task.OutputComponent() { Type = MyCodableConcept, Value = FhirStringValue });
      }

      //Update Task InProgress
      if (IFhirSpecificationDefinitionLoaderParameters.TaskStatus == Task.TaskStatus.InProgress && !string.IsNullOrWhiteSpace(IFhirSpecificationDefinitionLoaderParameters.FileInProgress))
      {
        var FhirStringValue = new FhirString(IFhirSpecificationDefinitionLoaderParameters.FileInProgress);
        var MyCodableConcept = new CodeableConcept(_TaskStatusSystem, Task.TaskStatus.InProgress.GetLiteral(), Task.TaskStatus.InProgress.GetLiteral());
        Task.Output.Add(new Task.OutputComponent() { Type = MyCodableConcept, Value = FhirStringValue });
      }

      //Update Task Failed
      if (IFhirSpecificationDefinitionLoaderParameters.TaskStatus == Task.TaskStatus.Failed && !string.IsNullOrWhiteSpace(IFhirSpecificationDefinitionLoaderParameters.FileInError))
      {
        var FhirStringValue = new FhirString($"{IFhirSpecificationDefinitionLoaderParameters.FileInError}: {IFhirSpecificationDefinitionLoaderParameters.ErrorMessage}");
        var MyCodableConcept = new CodeableConcept(_TaskStatusSystem, Task.TaskStatus.Failed.GetLiteral(), Task.TaskStatus.Failed.GetLiteral());
        Task.Output.Add(new Task.OutputComponent() { Type = MyCodableConcept, Value = FhirStringValue });
      }

      //Update Task Failed
      if (IFhirSpecificationDefinitionLoaderParameters.TaskStatus == Task.TaskStatus.Failed && string.IsNullOrWhiteSpace(IFhirSpecificationDefinitionLoaderParameters.FileInError) && !string.IsNullOrWhiteSpace(IFhirSpecificationDefinitionLoaderParameters.ErrorMessage))
      {
        var FhirStringValue = new FhirString(IFhirSpecificationDefinitionLoaderParameters.ErrorMessage);
        var MyCodableConcept = new CodeableConcept(_TaskStatusSystem, Task.TaskStatus.Failed.GetLiteral(), Task.TaskStatus.Failed.GetLiteral());
        Task.Output.Add(new Task.OutputComponent() { Type = MyCodableConcept, Value = FhirStringValue });
      }
    }
    
    private string GetZipFilePath()
    {

      if (HostingEnvironment.IsHosted)
      {
        //If running is IIS either IIS Express in dev or IIS in production i.e (Pyro.Web)
        return Path.Combine(AppContext.BaseDirectory, @"bin\ZipArchiveFiles\" + _ZipFileName);
      }
      else
      {
        //If running as a console server i.e (Pyro.ConsoleServer)
        return Path.Combine(AppContext.BaseDirectory, @"ZipArchiveFiles\" + _ZipFileName);
      }
    }

  }
}
