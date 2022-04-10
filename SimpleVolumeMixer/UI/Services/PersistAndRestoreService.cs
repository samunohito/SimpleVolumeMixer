using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleVolumeMixer.Core.Contracts.Services;
using SimpleVolumeMixer.Properties;
using SimpleVolumeMixer.UI.Contracts.Services;
using SimpleVolumeMixer.UI.Models;

namespace SimpleVolumeMixer.UI.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public class PersistAndRestoreService : IPersistAndRestoreService
{
    private readonly AppConfig _appConfig;
    private readonly IFileService _fileService;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public PersistAndRestoreService(IFileService fileService, AppConfig appConfig, IConfiguration c)
    {
        _fileService = fileService;
        _appConfig = appConfig;
    }

    public void PersistData()
    {
        var folderPath = GetFolder();
        var fileName = GetFileName();
        _fileService.Save(folderPath, fileName, Application.Current.Properties);
    }

    public void RestoreData()
    {
        var folderPath = GetFolder();
        var fileName = GetFileName();
        var properties = _fileService.Read<IDictionary>(folderPath, fileName);
        if (properties != null)
        {
            foreach (DictionaryEntry property in properties)
            {
                Application.Current.Properties.Add(property.Key, property.Value);
            }
        }
    }

    private string GetFolder()
    {
        var assembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException();
        var appLocation = Path.GetDirectoryName(assembly.Location) ?? "";
        return Path.Combine(_localAppData, _appConfig.ConfigurationsFolder ?? appLocation);
    }

    private string GetFileName()
    {
        return _appConfig.AppPropertiesFileName ?? Resources.AppConfigFileName;
    }
}