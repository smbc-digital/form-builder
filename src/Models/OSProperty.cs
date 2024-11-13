namespace form_builder.Models
{
    public class Header
    {
        public string uri { get; set; }
        public string query { get; set; }
        public int offset { get; set; }
        public int totalresults { get; set; }
        public string format { get; set; }
        public string dataset { get; set; }
        public string lr { get; set; }
        public int maxresults { get; set; }
        public int matchprecision { get; set; }
        public string epoch { get; set; }
        public string lastupdate { get; set; }
        public string filter { get; set; }
        public string output_srs { get; set; }
    }

    public class LPI
    {
        public string UPRN { get; set; }
        public string ADDRESS { get; set; }
        public string USRN { get; set; }
        public string LPI_KEY { get; set; }
        public string PAO_START_NUMBER { get; set; }
        public string STREET_DESCRIPTION { get; set; }
        public string LOCALITY_NAME { get; set; }
        public string TOWN_NAME { get; set; }
        public string ADMINISTRATIVE_AREA { get; set; }
        public string POSTCODE_LOCATOR { get; set; }
        public string RPC { get; set; }
        public double X_COORDINATE { get; set; }
        public double Y_COORDINATE { get; set; }
        public string STATUS { get; set; }
        public string LOGICAL_STATUS_CODE { get; set; }
        public string CLASSIFICATION_CODE { get; set; }
        public string CLASSIFICATION_CODE_DESCRIPTION { get; set; }
        public int LOCAL_CUSTODIAN_CODE { get; set; }
        public string LOCAL_CUSTODIAN_CODE_DESCRIPTION { get; set; }
        public string COUNTRY_CODE { get; set; }
        public string COUNTRY_CODE_DESCRIPTION { get; set; }
        public string POSTAL_ADDRESS_CODE { get; set; }
        public string POSTAL_ADDRESS_CODE_DESCRIPTION { get; set; }
        public string BLPU_STATE_CODE { get; set; }
        public string BLPU_STATE_CODE_DESCRIPTION { get; set; }
        public string TOPOGRAPHY_LAYER_TOID { get; set; }
        public string WARD_CODE { get; set; }
        public string LAST_UPDATE_DATE { get; set; }
        public string ENTRY_DATE { get; set; }
        public string BLPU_STATE_DATE { get; set; }
        public string STREET_STATE_CODE { get; set; }
        public string STREET_STATE_CODE_DESCRIPTION { get; set; }
        public string STREET_CLASSIFICATION_CODE { get; set; }
        public string STREET_CLASSIFICATION_CODE_DESCRIPTION { get; set; }
        public string LPI_LOGICAL_STATUS_CODE { get; set; }
        public string LPI_LOGICAL_STATUS_CODE_DESCRIPTION { get; set; }
        public string LANGUAGE { get; set; }
        public double MATCH { get; set; }
        public string MATCH_DESCRIPTION { get; set; }
    }

    public class Result
    {
        public LPI LPI { get; set; }
    }

    public class OSProperty
    {
        public Header header { get; set; }
        public List<Result> results { get; set; }
    }

}
