namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class AutocompleteSelectViewModel
    {
        public string InputName { get; set; } = string.Empty;
        public int? SelectedId { get; set; }
        public string SelectedText { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string RequiredMessage { get; set; } = string.Empty;
    }
}
