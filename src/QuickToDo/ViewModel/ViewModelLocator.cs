/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:QuickToDo"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System;
using System.Net;
using System.Configuration;
using GalaSoft.MvvmLight.Ioc;
using HabitRPG.Client;
using QuickToDo.Services;
using Microsoft.Practices.ServiceLocation;
using QuickToDo.Infrastructure;

namespace QuickToDo.ViewModel
{
  /// <summary>
  /// This class contains static references to all the view models in the
  /// application and provides an entry point for the bindings.
  /// </summary>
  public class ViewModelLocator
  {
    /// <summary>
    /// Initializes a new instance of the ViewModelLocator class.
    /// </summary>
    public ViewModelLocator()
    {
      ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
      
      if (Properties.Settings.Default.ServiceType == "Wunderlist")
      {
        SimpleIoc.Default.Register<ITaskService>(() => new WunderlistService(Properties.Settings.Default.ApiToken, Properties.Settings.Default.UserId));
      }
      else if (Properties.Settings.Default.ServiceType == "Habitica")
      {
        SimpleIoc.Default.Register<IUserClient>(() =>
        {
          var proxy = WebRequest.DefaultWebProxy;
          proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

          var configuration = new HabitRpgConfiguration()
          {
            ApiToken = Guid.Parse(Properties.Settings.Default.ApiToken),
            ServiceUri = new Uri(ConfigurationManager.AppSettings["HabiticaApiUri"]),
            UserId = Guid.Parse(Properties.Settings.Default.UserId)
          };

          return new UserClient(configuration, proxy);
        });
        
        SimpleIoc.Default.Register<ITaskService, HabiticaService>();
      }

      SimpleIoc.Default.Register<ISettingsService, Settings>();
      SimpleIoc.Default.Register<IDialogService, DialogService>();
      SimpleIoc.Default.Register<IAnalyticsTracker, AnalyticsTracker>();

      SimpleIoc.Default.Register<MainViewModel>();
      SimpleIoc.Default.Register<SettingsViewModel>();
    }

    public MainViewModel Main
    {
      get
      {
        return ServiceLocator.Current.GetInstance<MainViewModel>();
      }
    }

    public SettingsViewModel Settings
    {
      get
      {
        return ServiceLocator.Current.GetInstance<SettingsViewModel>();
      }
    }

    public static void Cleanup()
    {
      // TODO Clear the ViewModels
    }
  }
}