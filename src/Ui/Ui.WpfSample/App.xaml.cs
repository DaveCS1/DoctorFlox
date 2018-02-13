﻿namespace devdeer.DoctorFlox.Ui.WpfSample
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    using Autofac;

    using Helpers;

    using Interfaces;

    using Locators;

    using Logic.WpfSample.Helpers;
    using Logic.WpfSample.ViewModels;

    using Messages;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region constructors and destructors

        public App()
        {
            DispatcherHelper.Initialize();
            var builder = new ContainerBuilder();
            builder.Register(m => Messenger.Default).As<IMessenger>();
            builder.Register(s => SynchronizationContext.Current).As<SynchronizationContext>();
            // register all windows
            Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsPublic && typeof(Window).IsAssignableFrom(t)).ToList().ForEach(
                windowType =>
                {
                    builder.RegisterType(windowType).InstancePerDependency();
                });
            // register all view models
            typeof(MainViewModel).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsPublic && typeof(BaseViewModel).IsAssignableFrom(t)).ToList().ForEach(
                viewModelType =>
                {
                    builder.RegisterType(viewModelType).UsingConstructor(typeof(IMessenger), typeof(SynchronizationContext)).InstancePerDependency()
                        .OnActivated(vm => ((BaseViewModel)vm.Instance).OnInstanceActivated()).OnActivating(vm => ((BaseViewModel)vm.Instance).OnInstanceActivating());
                });
            // build container and assign instance
            Variables.AutoFacContainer = builder.Build();
            // override default view model factory with AutoFac
            ViewModelLocationProvider.SetDefaultViewModelFactory(type => Variables.AutoFacContainer.Resolve(type));
        }

        #endregion
    }
}