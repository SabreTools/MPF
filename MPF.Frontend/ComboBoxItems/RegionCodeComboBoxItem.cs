using System;
using System.Collections.Generic;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.ComboBoxItems
{
    /// <summary>
    /// Represents a single item in the Region combo box
    /// </summary>
    public class RegionCodeComboBoxItem : IEquatable<RegionCodeComboBoxItem>, IElement
    {
        #region IElement Implementation

        /// <inheritdoc/>
        public string Name => Data.Name;

        /// <inheritdoc/>
        public bool IsChecked
        {
            get;
            set
            {
                field = value;
                Checked(this);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Internal enum value
        /// </summary>
        public RegionCode Value => Data;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the checked state is changed
        /// </summary>
        public event SelectionChanged Checked = (_) => { return; };

        #endregion

        #region Private Fields

        /// <summary>
        /// Backing data for the item
        /// </summary>
        private readonly RegionCode Data;

        /// <summary>
        /// List of Redump-supported Regions
        /// </summary>
        private static readonly List<RegionCode> RedumpRegions =
        [
            RegionCode.Argentina,
            RegionCode.Asia,
            RegionCode.Australia,
            RegionCode.Austria,
            RegionCode.Azerbaijan,
            RegionCode.Belarus,
            RegionCode.Belgium,
            RegionCode.Brazil,
            RegionCode.Bulgaria,
            RegionCode.Canada,
            RegionCode.Chile,
            RegionCode.China,
            RegionCode.Croatia,
            RegionCode.Cyprus,
            RegionCode.Czechia,
            RegionCode.Denmark,
            RegionCode.Estonia,
            RegionCode.Europe,
            RegionCode.Export,
            RegionCode.Finland,
            RegionCode.France,
            RegionCode.Germany,
            RegionCode.Greece,
            RegionCode.Greenland,
            RegionCode.HongKong,
            RegionCode.Hungary,
            RegionCode.Iceland,
            RegionCode.India,
            RegionCode.Indonesia,
            RegionCode.Iran,
            RegionCode.Ireland,
            RegionCode.Israel,
            RegionCode.Italy,
            RegionCode.Japan,
            RegionCode.Jordan,
            RegionCode.SouthKorea,
            RegionCode.LatinAmerica,
            RegionCode.Lithuania,
            RegionCode.Malaysia,
            RegionCode.Mexico,
            RegionCode.Netherlands,
            RegionCode.NewZealand,
            RegionCode.Norway,
            RegionCode.Philippines,
            RegionCode.Poland,
            RegionCode.Portugal,
            RegionCode.Romania,
            RegionCode.RussianFederation,
            RegionCode.SaudiArabia,
            RegionCode.Scandinavia,
            RegionCode.Serbia,
            RegionCode.Singapore,
            RegionCode.Slovakia,
            RegionCode.SouthAfrica,
            RegionCode.Spain,
            RegionCode.Sweden,
            RegionCode.Switzerland,
            RegionCode.Taiwan,
            RegionCode.Thailand,
            RegionCode.Turkey,
            RegionCode.UnitedKingdom,
            RegionCode.Ukraine,
            RegionCode.UnitedArabEmirates,
            RegionCode.Ukraine,
            RegionCode.UnitedStatesOfAmerica,
            RegionCode.VietNam,
            RegionCode.World,
        ];

        #endregion

        public RegionCodeComboBoxItem(RegionCode data) => Data = data;

        /// <inheritdoc/>
        public override string ToString() => Name;

        #region Operators

        /// <summary>
        /// Allow elements to be used as their internal type
        /// </summary>
        public static implicit operator RegionCode?(RegionCodeComboBoxItem item) => item.Data;

        #endregion

        #region Generation

        /// <summary>
        /// Generate all elements associated with the data type
        /// </summary>
        /// <param name="limited">Indicates if a limited, Redump-supported set should be returned</param>
        public static List<RegionCodeComboBoxItem> GenerateElements(bool limited = false)
        {
            // If using a limited, Redump-supported set
            if (limited)
                return RedumpRegions.ConvertAll(r => new RegionCodeComboBoxItem(r));

            return [.. Array.ConvertAll(RegionCode.AllRegions, e => new RegionCodeComboBoxItem(e))];
        }

        #endregion

        #region IEquatable Implementation

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as RegionCodeComboBoxItem);
        }

        /// <inheritdoc/>
        public bool Equals(RegionCodeComboBoxItem? other)
        {
            if (other is null)
                return false;

            return Name == other.Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
