namespace MobilePhoneServiceAndSalesSystem.Models
{
    public class DateTimePickerViewModel
    {
        public string PropertyName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public System.DateTime? Value { get; set; }
        public bool IncludeTime { get; set; } = true;
    }
}
