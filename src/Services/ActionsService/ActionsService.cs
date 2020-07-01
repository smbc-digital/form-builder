using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.ActionsService
{
    public class ActionsService : IActionsService
    {
        private readonly IGateway _gateway;

        public ActionsService(IGateway gateway)
        {
            _gateway = gateway;
        }

        public void Process()
        {
            throw new System.NotImplementedException();
        }
    }
}