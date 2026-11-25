namespace MPF.Frontend.ComboBoxItems
{
    public interface IElement
    {
        /// <summary>
        /// Display name for the combo box element
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Determine if the item is selected or not
        /// </summary>
        /// <remarks>Only applies to CheckBox types and can be ignored otherwise</remarks>
        public bool IsChecked { get; set; }
    }
}
