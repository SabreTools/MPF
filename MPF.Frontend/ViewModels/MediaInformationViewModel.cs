using System;
using System.Collections.Generic;
using MPF.Frontend.ComboBoxItems;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.ViewModels
{
    public class MediaInformationViewModel
    {
        #region Fields

        /// <summary>
        /// Application-level Options object
        /// </summary>
        public Options Options { get; private set; }

        #region Common Media Information

        public string? Title
        {
            get => SubmissionInfo.DiscIdentity.Title;
            set => SubmissionInfo.DiscIdentity.Title = value;
        }

        public string? AlternativeTitle
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.AlternativeTitle, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.AlternativeTitle] = value ?? string.Empty;
        }

        public string? ForeignTitle
        {
            get => SubmissionInfo.DiscIdentity.ForeignTitle;
            set => SubmissionInfo.DiscIdentity.ForeignTitle = value;
        }

        public string? AlternativeForeignTitle
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.AlternativeForeignTitle, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.AlternativeForeignTitle] = value ?? string.Empty;
        }

        public string? DiscNumber
        {
            get => SubmissionInfo.DiscIdentity.DiscNumber;
            set => SubmissionInfo.DiscIdentity.DiscNumber = value;
        }

        public string? DiscTitle
        {
            get => SubmissionInfo.DiscIdentity.DiscTitle;
            set => SubmissionInfo.DiscIdentity.DiscTitle = value;
        }

        public string? DiscTitleNonLatin
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.DiscTitleNonLatin, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.DiscTitleNonLatin] = value ?? string.Empty;
        }

        public DiscCategory? Category
        {
            get => SubmissionInfo.DiscIdentity.Category;
            set => SubmissionInfo.DiscIdentity.Category = value;
        }

        public string? DiscSerials
        {
            get => SubmissionInfo.DiscIdentifiers.DiscSerials;
            set => SubmissionInfo.DiscIdentifiers.DiscSerials = value;
        }

        public string? Barcodes
        {
            get => SubmissionInfo.DiscIdentifiers.Barcodes;
            set => SubmissionInfo.DiscIdentifiers.Barcodes = value;
        }

        #endregion

        #region Versions and Editions

        public string? Version
        {
            get => SubmissionInfo.DiscIdentifiers.Version;
            set => SubmissionInfo.DiscIdentifiers.Version = value;
        }

        public string? Editions
        {
            get => SubmissionInfo.DiscIdentifiers.Editions;
            set => SubmissionInfo.DiscIdentifiers.Editions = value;
        }

        public string? EditionNonLatin
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.EditionNonLatin, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.EditionNonLatin] = value ?? string.Empty;
        }

        #endregion

        #region Extras

        public bool PCMacHybrid
        {
            get => SubmissionInfo.DumpMetadata.CommentsSpecialFields.ContainsKey(SiteCode.PCMacHybrid);
            set
            {
                if (value)
                    SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.PCMacHybrid] = true.ToString();
                else
                    SubmissionInfo.DumpMetadata.CommentsSpecialFields.Remove(SiteCode.PCMacHybrid);
            }
        }

        public string? Comments
        {
            get => SubmissionInfo.DumpMetadata.Comments;
            set => SubmissionInfo.DumpMetadata.Comments = value;
        }

        public string? CompatibleOS
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.CompatibleOS, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.CompatibleOS] = value ?? string.Empty;
        }

        public string? DiscKey
        {
            get => SubmissionInfo.DiscIdentifiers.DiscKey;
            set => SubmissionInfo.DiscIdentifiers.DiscKey = value;
        }

        public string? InternalDiscID
        {
            get => SubmissionInfo.DiscIdentifiers.DiscID;
            set => SubmissionInfo.DiscIdentifiers.DiscID = value;
        }

        public string? Protection
        {
            get => SubmissionInfo.DumpMetadata.Protection;
            set => SubmissionInfo.DumpMetadata.Protection = value;
        }

        #endregion

        #region Physical Identifiers

        public string? BBFCRegistrationNumber
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.BBFCRegistrationNumber, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.BBFCRegistrationNumber] = value ?? string.Empty;
        }

        public string? CoverID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.CoverID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.CoverID] = value ?? string.Empty;
        }

        public string? DiscHologramID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.DiscHologramID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.DiscHologramID] = value ?? string.Empty;
        }

        public string? DNASDiscID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.DNASDiscID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.DNASDiscID] = value ?? string.Empty;
        }

        public string? DiscID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.DiscID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.DiscID] = value ?? string.Empty;
        }

        public string? ISBN
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.ISBN, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.ISBN] = value ?? string.Empty;
        }

        public string? ISSN
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.ISSN, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.ISSN] = value ?? string.Empty;
        }

        public string? PPN
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.PPN, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.PPN] = value ?? string.Empty;
        }

        public string? VFCCode
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.VFCCode, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.VFCCode] = value ?? string.Empty;
        }

        #endregion

        #region Publisher Identifiers

        public string? TwoKGamesID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.TwoKGamesID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.TwoKGamesID] = value ?? string.Empty;
        }

        public string? AcclaimID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.AcclaimID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.AcclaimID] = value ?? string.Empty;
        }

        public string? AccoladeID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.AccoladeID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.AccoladeID] = value ?? string.Empty;
        }

        public string? ActivisionID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.ActivisionID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.ActivisionID] = value ?? string.Empty;
        }

        public string? BandaiID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.BandaiID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.BandaiID] = value ?? string.Empty;
        }

        public string? BethesdaID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.BethesdaID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.BethesdaID] = value ?? string.Empty;
        }

        public string? CDProjektID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.CDProjektID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.CDProjektID] = value ?? string.Empty;
        }

        public string? DisneyInteractiveID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.DisneyInteractiveID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.DisneyInteractiveID] = value ?? string.Empty;
        }

        public string? EidosID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.EidosID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.EidosID] = value ?? string.Empty;
        }

        public string? ElectronicArtsID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.ElectronicArtsID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.ElectronicArtsID] = value ?? string.Empty;
        }

        public string? FoxInteractiveID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.FoxInteractiveID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.FoxInteractiveID] = value ?? string.Empty;
        }

        public string? GTInteractiveID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.GTInteractiveID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.GTInteractiveID] = value ?? string.Empty;
        }

        public string? InterplayID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.InterplayID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.InterplayID] = value ?? string.Empty;
        }

        public string? JASRACID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.JASRACID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.JASRACID] = value ?? string.Empty;
        }

        public string? KingRecordsID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.KingRecordsID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.KingRecordsID] = value ?? string.Empty;
        }

        public string? KoeiID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.KoeiID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.KoeiID] = value ?? string.Empty;
        }

        public string? KonamiID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.KonamiID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.KonamiID] = value ?? string.Empty;
        }

        public string? LucasArtsID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.LucasArtsID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.LucasArtsID] = value ?? string.Empty;
        }

        public string? MicrosoftID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.MicrosoftID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.MicrosoftID] = value ?? string.Empty;
        }

        public string? NaganoID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.NaganoID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.NaganoID] = value ?? string.Empty;
        }

        public string? NamcoID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.NamcoID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.NamcoID] = value ?? string.Empty;
        }

        public string? NipponIchiSoftwareID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.NipponIchiSoftwareID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.NipponIchiSoftwareID] = value ?? string.Empty;
        }

        public string? OriginID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.OriginID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.OriginID] = value ?? string.Empty;
        }

        public string? PonyCanyonID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.PonyCanyonID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.PonyCanyonID] = value ?? string.Empty;
        }

        public string? SegaID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.SegaID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.SegaID] = value ?? string.Empty;
        }

        public string? SelenID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.SelenID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.SelenID] = value ?? string.Empty;
        }

        public string? SierraID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.SierraID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.SierraID] = value ?? string.Empty;
        }

        public string? TaitoID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.TaitoID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.TaitoID] = value ?? string.Empty;
        }

        public string? UbisoftID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.UbisoftID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.UbisoftID] = value ?? string.Empty;
        }

        public string? ValveID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.ValveID, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.CommentsSpecialFields[SiteCode.ValveID] = value ?? string.Empty;
        }

        #endregion

        #region Contents

        public string? GeneralContent
        {
            get => SubmissionInfo.DumpMetadata.Contents;
            set => SubmissionInfo.DumpMetadata.Contents = value;
        }

        public string? Applications
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.Applications, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.Applications] = value ?? string.Empty;
        }

        public string? Games
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.Games, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.Games] = value ?? string.Empty;
        }

        public string? NetYarozeGames
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.NetYarozeGames, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.NetYarozeGames] = value ?? string.Empty;
        }

        public string? PlayableDemos
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.PlayableDemos, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.PlayableDemos] = value ?? string.Empty;
        }

        public string? RollingDemos
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.RollingDemos, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.RollingDemos] = value ?? string.Empty;
        }

        public string? TechDemos
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.TechDemos, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.TechDemos] = value ?? string.Empty;
        }

        public string? GameFootage
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.GameFootage, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.GameFootage] = value ?? string.Empty;
        }

        public string? Videos
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.Videos, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.Videos] = value ?? string.Empty;
        }

        public string? Patches
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.Patches, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.Patches] = value ?? string.Empty;
        }

        public string? Savegames
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.Savegames, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.Savegames] = value ?? string.Empty;
        }

        public string? Extras
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.ContentsSpecialFields.TryGetValue(SiteCode.Extras, out var value))
                    return value;

                return null;
            }
            set => SubmissionInfo.DumpMetadata.ContentsSpecialFields[SiteCode.Extras] = value ?? string.Empty;
        }

        #endregion

        #region Ringcodes

        #region Layer 0

        public string? Layer0MasteringCode
        {
            get => SubmissionInfo.RingCodes.Layer0MasteringCode;
            set => SubmissionInfo.RingCodes.Layer0MasteringCode = value;
        }

        public string? Layer0MasteringSID
        {
            get => SubmissionInfo.RingCodes.Layer0MasteringSID;
            set => SubmissionInfo.RingCodes.Layer0MasteringSID = value;
        }

        public string? Layer0Toolstamps
        {
            get => SubmissionInfo.RingCodes.Layer0Toolstamps;
            set => SubmissionInfo.RingCodes.Layer0Toolstamps = value;
        }

        public string? Layer0MouldSIDs
        {
            get => SubmissionInfo.RingCodes.Layer0MouldSIDs;
            set => SubmissionInfo.RingCodes.Layer0MouldSIDs = value;
        }

        public string? Layer0AdditionalMoulds
        {
            get => SubmissionInfo.RingCodes.Layer0AdditionalMoulds;
            set => SubmissionInfo.RingCodes.Layer0AdditionalMoulds = value;
        }

        #endregion

        #region Layer 1

        public string? Layer1MasteringCode
        {
            get => SubmissionInfo.RingCodes.Layer1MasteringCode;
            set => SubmissionInfo.RingCodes.Layer1MasteringCode = value;
        }

        public string? Layer1MasteringSID
        {
            get => SubmissionInfo.RingCodes.Layer1MasteringSID;
            set => SubmissionInfo.RingCodes.Layer1MasteringSID = value;
        }

        public string? Layer1Toolstamps
        {
            get => SubmissionInfo.RingCodes.Layer1Toolstamps;
            set => SubmissionInfo.RingCodes.Layer1Toolstamps = value;
        }

        #endregion

        #region Layer 2

        public string? Layer2MasteringCode
        {
            get => SubmissionInfo.RingCodes.Layer2MasteringCode;
            set => SubmissionInfo.RingCodes.Layer2MasteringCode = value;
        }

        public string? Layer2MasteringSID
        {
            get => SubmissionInfo.RingCodes.Layer2MasteringSID;
            set => SubmissionInfo.RingCodes.Layer2MasteringSID = value;
        }

        public string? Layer2Toolstamps
        {
            get => SubmissionInfo.RingCodes.Layer2Toolstamps;
            set => SubmissionInfo.RingCodes.Layer2Toolstamps = value;
        }

        #endregion

        #region Layer 3

        public string? Layer3MasteringCode
        {
            get => SubmissionInfo.RingCodes.Layer3MasteringCode;
            set => SubmissionInfo.RingCodes.Layer3MasteringCode = value;
        }

        public string? Layer3MasteringSID
        {
            get => SubmissionInfo.RingCodes.Layer3MasteringSID;
            set => SubmissionInfo.RingCodes.Layer3MasteringSID = value;
        }

        public string? Layer3Toolstamps
        {
            get => SubmissionInfo.RingCodes.Layer3Toolstamps;
            set => SubmissionInfo.RingCodes.Layer3Toolstamps = value;
        }

        #endregion

        #region Label Side

        public string? LabelSideMasteringCode
        {
            get => SubmissionInfo.RingCodes.LabelSideMasteringCode;
            set => SubmissionInfo.RingCodes.LabelSideMasteringCode = value;
        }

        public string? LabelSideMasteringSID
        {
            get => SubmissionInfo.RingCodes.LabelSideMasteringSID;
            set => SubmissionInfo.RingCodes.LabelSideMasteringSID = value;
        }

        public string? LabelSideToolstamps
        {
            get => SubmissionInfo.RingCodes.LabelSideToolstamps;
            set => SubmissionInfo.RingCodes.LabelSideToolstamps = value;
        }

        public string? LabelSideMouldSIDs
        {
            get => SubmissionInfo.RingCodes.LabelSideMouldSIDs;
            set => SubmissionInfo.RingCodes.LabelSideMouldSIDs = value;
        }

        public string? LabelSideAdditionalMoulds
        {
            get => SubmissionInfo.RingCodes.LabelSideAdditionalMoulds;
            set => SubmissionInfo.RingCodes.LabelSideAdditionalMoulds = value;
        }

        #endregion

        #endregion

        #region Read-Only Info

        public string? FullyMatchedIDs
        {
            get
            {
                if (SubmissionInfo.FullyMatchedIDs is null)
                    return null;

                return string.Join(", ", [.. SubmissionInfo.FullyMatchedIDs.ConvertAll(i => i.ToString())]);
            }
        }

        public string? PartiallyMatchedIDs
        {
            get
            {
                if (SubmissionInfo.PartiallyMatchedIDs is null)
                    return null;

                return string.Join(", ", [.. SubmissionInfo.PartiallyMatchedIDs.ConvertAll(i => i.ToString())]);
            }
        }

        public string? Dat
        {
            get => SubmissionInfo.DumpMetadata.Dat;
        }

        public string? Layerbreak
        {
            get
            {
                if (SubmissionInfo.DiscIdentifiers.Layerbreak == default)
                    return null;

                return SubmissionInfo.DiscIdentifiers.Layerbreak.ToString();
            }
        }

        public string? Layerbreak2
        {
            get
            {
                if (SubmissionInfo.DiscIdentifiers.Layerbreak2 == default)
                    return null;

                return SubmissionInfo.DiscIdentifiers.Layerbreak2.ToString();
            }
        }

        public string? Layerbreak3
        {
            get
            {
                if (SubmissionInfo.DiscIdentifiers.Layerbreak3 == default)
                    return null;

                return SubmissionInfo.DiscIdentifiers.Layerbreak3.ToString();
            }
        }

        public string? WriteOffset
        {
            get => SubmissionInfo.RingCodes.WriteOffset;
        }

        public string? DMIHash
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.DMIHash, out var value))
                    return value;

                return null;
            }
        }

        public string? EDC
        {
            get => SubmissionInfo.DiscIdentifiers.EDC.ToString();
        }

        public string? ErrorCount
        {
            get => SubmissionInfo.DiscIdentifiers.ErrorCount;
        }

        public string? EXEDate
        {
            get => SubmissionInfo.DiscIdentifiers.EXEDate;
        }

        public string? Filename
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.Filename, out var value))
                    return value;

                return null;
            }
        }

        public string? Header
        {
            get => SubmissionInfo.DumpMetadata.Header;
        }

        public string? InternalName
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.InternalName, out var value))
                    return value;

                return null;
            }
        }

        public string? InternalSerialName
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.InternalSerialName, out var value))
                    return value;

                return null;
            }
        }

        public string? Multisession
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.Multisession, out var value))
                    return value;

                return null;
            }
        }

        public string? PFIHash
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.PFIHash, out var value))
                    return value;

                return null;
            }
        }

        public string? PIC
        {
            get => SubmissionInfo.DumpMetadata.PIC;
        }

        public string? PVD
        {
            get => SubmissionInfo.DumpMetadata.PVD;
        }

        public string? RingPerfectAudioOffset
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.RingPerfectAudioOffset, out var value))
                    return value;

                return null;
            }
        }

        public string? SampleStart
        {
            get => SubmissionInfo.RingCodes.SampleStart;
        }

        public string? SBI
        {
            get => SubmissionInfo.DumpMetadata.SBI;
        }

        public string? SSHash
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.SSHash, out var value))
                    return value;

                return null;
            }
        }

        public string? SectorRanges
        {
            get => SubmissionInfo.DumpMetadata.SectorRanges;
        }

        public string? SSVersion
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.SSVersion, out var value))
                    return value;

                return null;
            }
        }

        public string? UniversalHash
        {
            get => SubmissionInfo.DiscIdentifiers.UniversalHash;
        }

        public string? VolumeLabel
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.VolumeLabel, out var value))
                    return value;

                return null;
            }
        }

        public string? XeMID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.XeMID, out var value))
                    return value;

                return null;
            }
        }

        public string? XMID
        {
            get
            {
                if (SubmissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(SiteCode.XMID, out var value))
                    return value;

                return null;
            }
        }

        #endregion

        #endregion

        #region Selection Lists

        /// <summary>
        /// List of available disc categories
        /// </summary>
        public List<Element<DiscCategory>> Categories { get; private set; }
            = Element<DiscCategory>.GenerateElements();

        /// <summary>
        /// List of available regions
        /// </summary>
        public List<RegionCodeComboBoxItem> Regions { get; private set; }
             = RegionCodeComboBoxItem.GenerateElements();

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<LanguageCodeComboBoxItem> Languages { get; private set; }
            = LanguageCodeComboBoxItem.GenerateElements();

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
            RegionCode.Hungary,
            RegionCode.Iceland,
            RegionCode.India,
            RegionCode.Indonesia,
            RegionCode.Iran,
            RegionCode.Ireland,
            RegionCode.Israel,
            RegionCode.Italy,
            RegionCode.Japan,
            RegionCode.SouthKorea,
            RegionCode.LatinAmerica,
            RegionCode.Lithuania,
            RegionCode.Malaysia,
            RegionCode.Mexico,
            RegionCode.Netherlands,
            RegionCode.NewZealand,
            RegionCode.Norway,
            RegionCode.Poland,
            RegionCode.Portugal,
            RegionCode.Romania,
            RegionCode.RussianFederation,
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

        /// <summary>
        /// List of Redump-supported Languages
        /// </summary>
        private static readonly List<LanguageCode> RedumpLanguages =
        [
            LanguageCode.Afrikaans,
            LanguageCode.Albanian,
            LanguageCode.Arabic,
            LanguageCode.Armenian,
            LanguageCode.Azerbaijani,
            LanguageCode.Basque,
            LanguageCode.Belarusian,
            LanguageCode.Bulgarian,
            LanguageCode.Catalan,
            LanguageCode.Chinese,
            LanguageCode.Croatian,
            LanguageCode.Czech,
            LanguageCode.Danish,
            LanguageCode.Dutch,
            LanguageCode.English,
            LanguageCode.Estonian,
            LanguageCode.Finnish,
            LanguageCode.French,
            LanguageCode.Gaelic,
            LanguageCode.Galician,
            LanguageCode.German,
            LanguageCode.Greek,
            LanguageCode.Hebrew,
            LanguageCode.Hindi,
            LanguageCode.Hungarian,
            LanguageCode.Icelandic,
            LanguageCode.Indonesian,
            LanguageCode.Irish,
            LanguageCode.Italian,
            LanguageCode.Japanese,
            LanguageCode.Korean,
            LanguageCode.Latin,
            LanguageCode.Latvian,
            LanguageCode.Lithuanian,
            LanguageCode.Macedonian,
            LanguageCode.Malay,
            LanguageCode.Maori,
            LanguageCode.Norwegian,
            LanguageCode.Persian,
            LanguageCode.Polish,
            LanguageCode.Portuguese,
            LanguageCode.Panjabi,
            LanguageCode.Romanian,
            LanguageCode.Russian,
            LanguageCode.Serbian,
            LanguageCode.Slovak,
            LanguageCode.Slovenian,
            LanguageCode.Spanish,
            LanguageCode.Swedish,
            LanguageCode.Tamil,
            LanguageCode.Thai,
            LanguageCode.Turkish,
            LanguageCode.Ukrainian,
            LanguageCode.Vietnamese,
            LanguageCode.Welsh,
        ];

        #endregion

        #region Internal State

        /// <summary>
        /// SubmissionInfo object to fill and save
        /// </summary>
        public SubmissionInfo SubmissionInfo { get; private set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaInformationViewModel(Options options, SubmissionInfo? submissionInfo)
        {
            Options = options;
            SubmissionInfo = submissionInfo?.Clone() as SubmissionInfo ?? new SubmissionInfo();
        }

        #region Helpers

        /// <summary>
        /// Load the current contents of the base SubmissionInfo to the UI
        /// </summary>
        /// TODO: Convert selected list item to binding
        public void Load()
        {
            if (SubmissionInfo.RegionsAndLanguages.Regions is not null)
                Regions.ForEach(l => l.IsChecked = Array.IndexOf(SubmissionInfo.RegionsAndLanguages.Regions, l) > -1);
            if (SubmissionInfo.RegionsAndLanguages.Languages is not null)
                Languages.ForEach(l => l.IsChecked = Array.IndexOf(SubmissionInfo.RegionsAndLanguages.Languages, l) > -1);
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        /// TODO: Convert selected list item to binding
        public void Save()
        {
            SubmissionInfo.RegionsAndLanguages.Regions = [.. Regions.FindAll(l => l.IsChecked).ConvertAll(l => l?.Value)];
            if (SubmissionInfo.RegionsAndLanguages.Regions.Length == 0)
                SubmissionInfo.RegionsAndLanguages.Regions = [null];

            SubmissionInfo.RegionsAndLanguages.Languages = [.. Languages.FindAll(l => l.IsChecked).ConvertAll(l => l?.Value)];
            if (SubmissionInfo.RegionsAndLanguages.Languages.Length == 0)
                SubmissionInfo.RegionsAndLanguages.Languages = [null];

            SubmissionInfo.DiscIdentity.Title = FrontendTool.NormalizeDiscTitle(SubmissionInfo.DiscIdentity.Title, SubmissionInfo.RegionsAndLanguages.Languages);
        }

        /// <summary>
        /// Repopulate the list of Languages based on Redump support
        /// </summary>
        public void SetRedumpLanguages()
        {
            Languages = RedumpLanguages.ConvertAll(l => new LanguageCodeComboBoxItem(l));
        }

        /// <summary>
        /// Repopulate the list of Regions based on Redump support
        /// </summary>
        public void SetRedumpRegions()
        {
            Regions = RedumpRegions.ConvertAll(r => new RegionCodeComboBoxItem(r));
        }

        #endregion
    }
}
