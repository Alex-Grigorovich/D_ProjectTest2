using VContainer;
using VContainer.Unity;
using NetworkMessaging.Services;

namespace NetworkMessaging.DI
{
    public class NetworkMessageInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<NetworkMessageService>(Lifetime.Singleton)
                   .AsSelf();
        }
    }
}