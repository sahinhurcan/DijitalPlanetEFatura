public class DijitalPlanetSendEInvoice : ICommandSendEInvoice {

        private Task<ServiceResponse> _result;
        private IntegrationServiceSoapClient _serviceProxy;
        public InvoiceType Invoice { get; set; }
        public IMapper IMapper { get; set; }
        public ServiceInfo ServiceInfo { get; set; }

        public DijitalPlanetSendEInvoice(
            InvoiceType invoiceType = null,
            IMapper mapper = null) {
            this.IMapper = mapper;
            this.Invoice = invoiceType;
        }

        public Task<ServiceResponse> TaskResult() => this._result;

        public async Task Execute() {

            var result = new ServiceResponse() {
                Hatali = false,
            };

            this._serviceProxy = CreateServiceProxy(ServiceInfo.ServiceUrl, ServiceInfo.UserName, ServiceInfo.Password);

            var ticket = _serviceProxy.GetFormsAuthenticationTicket(
                    this.ServiceInfo.CorporateCode,
                    this.ServiceInfo.UserName,
                    this.ServiceInfo.Password);

            var invoiceRawData = Serialization.SerializeToBytes(this.Invoice);
            var receiverPostboxName = this.ServiceInfo.ReceiverPostboxName;

            var stateResult = _serviceProxy.SendUBLInvoice(ticket, invoiceRawData, this.ServiceInfo.CorporateCode, "", receiverPostboxName);

            result.Sonuc = stateResult.ErrorCode == 0 ? "İşlem başarıyla tamamlandı" : "İşlem tamamlandı!";
            result.Data = stateResult;

            _result = Task.FromResult(result);
        }

        private IntegrationServiceSoapClient CreateServiceProxy(string serviceUrl, string userName, string password) {
            var basicHttpBinding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            var endPoint = new EndpointAddress(serviceUrl);

            var proxy = new IntegrationServiceSoapClient(basicHttpBinding, endPoint);
            proxy.ClientCredentials.UserName.UserName = userName;
            proxy.ClientCredentials.UserName.Password = password;
            return proxy;
        }

        public void Dispose() {
            _result?.Dispose();
            (this._serviceProxy as ICommunicationObject)?.Close();
        }
    }