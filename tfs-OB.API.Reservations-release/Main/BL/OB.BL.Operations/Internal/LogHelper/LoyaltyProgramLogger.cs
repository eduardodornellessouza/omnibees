using OB.BL.Operations.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using contractsCRM = OB.Reservation.BL.Contracts.Data.CRM;
using contractsEvents = OB.Events.Contracts;

namespace OB.BL.Operations.Internal.LogHelper
{
    /// <summary>
    /// Class for generate log of LoyaltyPrograms.
    /// </summary>
    internal class LoyaltyProgramLogger : BaseLog
    {
        public IEnumerable<OB.Domain.General.Currency> AllCurrencies { get; set; }
        public Dictionary<long,Tuple<string,long>> ratesUIDNamePropertyUID{ get; set; }
        public Dictionary<long, string> propertiesUIDName { get; set; }

        public LoyaltyProgramLogger() { }


        /// <summary>
        /// Creates the log for a single LoyaltyProgram.
        /// </summary>
        public contractsEvents.Messages.Notification GenerateSingleLoyaltyProgramLog(contractsCRM.LoyaltyProgram currentProgram, contractsCRM.LoyaltyProgram previousProgram, long defaultLanguageUID, string defaultCurrencySymbol, Dictionary<byte, byte> migrateGuestsBetweenLevelsNrs)
        {
            if (currentProgram == null)
                return null;

            // If programs are equals the log it is not created
            if (this.ProgramsAreEquals(currentProgram, previousProgram))
                return null;

            // Set operation type
            var operation = contractsEvents.Operations.Modify;
            if (currentProgram.IsDeleted)
                operation = contractsEvents.Operations.Delete;
            else if (previousProgram == null)
                operation = contractsEvents.Operations.Create;

            if (previousProgram == null)
                previousProgram = new contractsCRM.LoyaltyProgram();

            var entityDeltas = new List<contractsEvents.EntityDelta>();

            #region Fill LoyaltyProgram Delta
            var programDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.LoyaltyProgram)
            {
                EntityKey = currentProgram.UID
            };
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Name, currentProgram.Name, previousProgram.Name);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Description, currentProgram.Description, previousProgram.Description);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.OnlySearchesByLoyaltyRates, currentProgram.OnlySearchesByLoyaltyRates, previousProgram.OnlySearchesByLoyaltyRates);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.BackgroundImageName, currentProgram.BackgroundImageName, previousProgram.BackgroundImageName);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.AttachmentPDFName, currentProgram.AttachmentPDFName, previousProgram.AttachmentPDFName);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsActive, currentProgram.IsActive, previousProgram.IsActive);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Client_UID, currentProgram.Client_UID, previousProgram.Client_UID);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CreatedByPropertyUID, currentProgram.CreatedByPropertyUID, previousProgram.CreatedByPropertyUID);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LanguageUID, defaultLanguageUID, defaultLanguageUID);
            programDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DefaultCurrencySymbol, defaultCurrencySymbol, defaultCurrencySymbol);

            if (operation == contractsEvents.Operations.Modify)
                programDelta.EntityState = contractsEvents.EntityState.Modified;
            else if (operation == contractsEvents.Operations.Create)
                programDelta.EntityState = contractsEvents.EntityState.Created;
            else if (operation == contractsEvents.Operations.Delete)
                programDelta.EntityState = contractsEvents.EntityState.Deleted;

            var privilegedProgramProperties = new List<contractsEvents.Enums.EntityPropertyEnum>()
            {
                contractsEvents.Enums.EntityPropertyEnum.Name,
                contractsEvents.Enums.EntityPropertyEnum.Client_UID,
                contractsEvents.Enums.EntityPropertyEnum.DefaultCurrencySymbol,
                contractsEvents.Enums.EntityPropertyEnum.LanguageUID,
                contractsEvents.Enums.EntityPropertyEnum.CreatedByPropertyUID
            };
            entityDeltas.Add(contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(programDelta, privilegedProgramProperties));
            #endregion

            #region Fill LoyaltyProgramLanguage Deltas

            var currentProgramLanguages = currentProgram.LoyaltyProgramsLanguages ?? new List<contractsCRM.LoyaltyProgramsLanguage>();
            var previouProgramLanguages = previousProgram.LoyaltyProgramsLanguages ?? new List<contractsCRM.LoyaltyProgramsLanguage>();

            // New or Modified LoyaltyProgramLanguages
            if (currentProgramLanguages.Any())
                foreach (var currentProgramLang in currentProgramLanguages)
                {
                    var previousProgramLang = previouProgramLanguages.FirstOrDefault(x => x.UID == currentProgramLang.UID);

                    var programLangState = contractsEvents.EntityState.Modified;
                    if (previousProgramLang == null)
                        programLangState = contractsEvents.EntityState.Created;

                    var programLangDelta = this.CreateLoyaltyProgramLanguageDelta(programLangState, currentProgramLang, previousProgramLang);
                    entityDeltas.Add(programLangDelta); // Create delta
                }

            // Removed LoyaltyProgramLanguages
            if (previouProgramLanguages.Any())
                foreach (var previousProgramLang in previouProgramLanguages)
                {
                    var currentProgramLang = previouProgramLanguages.FirstOrDefault(x => x.UID == previousProgramLang.UID);

                    if (currentProgramLang != null)
                        continue;

                    var programLangDelta = this.CreateLoyaltyProgramLanguageDelta(contractsEvents.EntityState.Deleted, currentProgramLang, previousProgramLang);
                    entityDeltas.Add(programLangDelta); // Create delta
                }

            #endregion

            #region LoyaltyLevel Deltas

            var currentLevels = currentProgram.LoyaltyLevels ?? new List<contractsCRM.LoyaltyLevel>();
            var previousLevels = (previousProgram.LoyaltyLevels ?? new List<contractsCRM.LoyaltyLevel>()).Where(x => !x.IsDeleted);
            var migrateGuestsToLevelNr = migrateGuestsBetweenLevelsNrs ?? new Dictionary<byte, byte>();

            // New or Modified LoyaltyLevel
            if (currentLevels.Any(x => !x.IsDeleted))
                foreach (var currentLevel in currentLevels.Where(x => !x.IsDeleted))
                {
                    var previousLevel = previousLevels.FirstOrDefault(x => x.UID == currentLevel.UID);

                    if (this.LevelsAreEquals(currentLevel, previousLevel) && !migrateGuestsToLevelNr.ContainsValue(currentLevel.LevelNr))
                        continue;

                    var levelState = contractsEvents.EntityState.Modified;
                    if (previousLevel == null)
                        levelState = contractsEvents.EntityState.Created;

                    // Migrate guests from a level to another level
                    byte destinationLevelNr;
                    migrateGuestsToLevelNr.TryGetValue(currentLevel.LevelNr, out destinationLevelNr);
                    long? destinationLevelUID = currentLevels.Where(x => !x.IsDeleted && x.LevelNr == destinationLevelNr).Select(x => x.UID).FirstOrDefault();

                    var levelDelta = this.CreateLoyaltyLevelDelta(levelState, currentLevel, previousLevel, destinationLevelUID > 0 ? destinationLevelUID : null);
                    entityDeltas.Add(levelDelta);

                    entityDeltas.AddRange(this.CreateAssociatedDeltasOfLoyaltyLevel(currentLevel, previousLevel)); // Create LoyaltyLevelCurrencies, LoyaltyLevelLanguages and RateLoyaltyLevels Deltas
                }

            // Removed LoyaltyLevel
            if (previousLevels.Any())
                foreach (var previousLevel in previousLevels)
                {
                    var currentLevel = currentLevels.FirstOrDefault(x => x.UID == previousLevel.UID);

                    if (currentLevel != null && !currentLevel.IsDeleted)
                        continue;

                    // Migrate guests from removed level to another existing level
                    byte destinationLevelNr;
                    migrateGuestsToLevelNr.TryGetValue(previousLevel.LevelNr, out destinationLevelNr);
                    long? destinationLevelUID = currentLevels.Where(x => !x.IsDeleted && x.LevelNr == destinationLevelNr).Select(x => x.UID).FirstOrDefault();

                    var levelDelta = this.CreateLoyaltyLevelDelta(contractsEvents.EntityState.Deleted, currentLevel, previousLevel, destinationLevelUID > 0 ? destinationLevelUID : null);
                    entityDeltas.Add(levelDelta);
                }

            #endregion

            // If LoyaltyProgram it is new then set all previous values to null
            if (operation == contractsEvents.Operations.Create)
                foreach (var property in entityDeltas.SelectMany(x => x.EntityProperties))
                    property.Value.PreviousValue = null;

            // Create informative EntityDeltas of Properties
            var propertyUIDs = entityDeltas.SelectMany(ed => ed.EntityProperties).Where(ep => ep.Key == contractsEvents.Enums.EntityPropertyEnum.PropertyUID.ToString()).Select(ep => (long)(ep.Value.CurrentValue ?? ep.Value.PreviousValue)).Distinct();
            var propertiesToCreateDeltas = this.propertiesUIDName.Where(x => propertyUIDs.Contains(x.Key));

            foreach (var propertyUIDName in propertiesToCreateDeltas)
                entityDeltas.Add(this.CreatePropertyDelta(propertyUIDName.Key, propertyUIDName.Value));

            // NOTIFICATION
            var notification = new contractsEvents.Messages.Notification()
            {
                EntityKey = currentProgram.UID,
                CreatedDate = DateTime.UtcNow,
                Action = contractsEvents.Action.LoyaltyPrograms,
                Operation = operation,
                PropertyUID = base.ModifiedByPropertyUID,
                PropertyName = base.ModifiedByPropertyName,
                CreatedBy = base.ModifiedByUserUID,
                CreatedByName = base.ModifiedByUserName,
                SubActions = new List<contractsEvents.SubAction>() { contractsEvents.SubAction.ConfigureLoyaltyProgram },
                EntityDeltas = contractsEvents.Helper.NotificationHelper.ClearNullOrEmptyEntityDeltas(entityDeltas) // clears null entity deltas
            };

            // Add sub actions according what was changed
            if (migrateGuestsBetweenLevelsNrs != null && migrateGuestsBetweenLevelsNrs.Any())
                notification.SubActions.Add(contractsEvents.SubAction.GuestVSLoyaltyLevel);
            if (notification.EntityDeltas.Any(x => x.EntityType == contractsEvents.Enums.EntityEnum.RateLoyaltyLevel.ToString()))
                notification.SubActions.Add(contractsEvents.SubAction.RateVSLoyaltyLevel);

            return notification;
        }


        private contractsEvents.EntityDelta CreateLoyaltyProgramLanguageDelta(contractsEvents.EntityState entityState, contractsCRM.LoyaltyProgramsLanguage currentLang, contractsCRM.LoyaltyProgramsLanguage previousLang)
        {
            bool isDeleted = entityState == contractsEvents.EntityState.Deleted;
            bool isCreated = entityState == contractsEvents.EntityState.Created;

            var programLanguageDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.LoyaltyProgramLanguage)
            {
                EntityKey = (entityState == contractsEvents.EntityState.Deleted) ? previousLang.UID : currentLang.UID,
                EntityState = entityState
            };

            programLanguageDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Program_UID, !isDeleted ? (long?)currentLang.LoyaltyProgram_UID : null, !isCreated ? (long?)previousLang.LoyaltyProgram_UID : null);
            programLanguageDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LanguageUID, !isDeleted ? (long?)currentLang.Language_UID : null, !isCreated ? (long?)previousLang.Language_UID : null);
            programLanguageDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Name, !isDeleted ? currentLang.Name : null, !isCreated ? previousLang.Name : null);
            programLanguageDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Description, !isDeleted ? currentLang.Description : null, !isCreated ? previousLang.Description : null);

            var privilegedProperties = new List<contractsEvents.Enums.EntityPropertyEnum>()
            {
                contractsEvents.Enums.EntityPropertyEnum.Program_UID,
                contractsEvents.Enums.EntityPropertyEnum.LanguageUID
            };
            return contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(programLanguageDelta, privilegedProperties, true);
        }

        private contractsEvents.EntityDelta CreateLoyaltyLevelDelta(contractsEvents.EntityState entityState, contractsCRM.LoyaltyLevel currentLevel, contractsCRM.LoyaltyLevel previousLevel, long? migrateGuestsToLevelUID = null)
        {
            if (currentLevel == null && previousLevel == null)
                return null;

            bool isDeletedLevel = entityState == contractsEvents.EntityState.Deleted;
            bool isCreatedLevel = entityState == contractsEvents.EntityState.Created;

            var levelDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.LoyaltyLevel)
            {
                EntityKey = (entityState == contractsEvents.EntityState.Deleted) ? previousLevel.UID : currentLevel.UID,
                EntityState = entityState
            };

            // If current level is null then consider equals to previous level
            var previousLevelCopy = IoHelper.Clone(previousLevel ?? new contractsCRM.LoyaltyLevel());
            var currentLevelCopy = IoHelper.Clone(currentLevel ?? previousLevel);

            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Program_UID, (long?)currentLevelCopy.LoyaltyProgram_UID, !isCreatedLevel ? (long?)previousLevelCopy.LoyaltyProgram_UID : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Name, currentLevelCopy.Name, !isCreatedLevel ? previousLevelCopy.Name : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Description, currentLevelCopy.Description, !isCreatedLevel ? previousLevelCopy.Description : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.DiscountValue, (decimal?)currentLevelCopy.DiscountValue, !isCreatedLevel ? (decimal?)previousLevelCopy.DiscountValue : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsPercentage, (bool?)currentLevelCopy.IsPercentage, !isCreatedLevel ? (bool?)previousLevelCopy.IsPercentage : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsActive, (bool?)currentLevelCopy.IsActive, !isCreatedLevel ? (bool?)previousLevelCopy.IsActive : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.AutomaticOptin, (bool?)currentLevelCopy.IsAutomatic, !isCreatedLevel ? (bool?)previousLevelCopy.IsAutomatic : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LevelNr, (byte?)currentLevelCopy.LevelNr, !isCreatedLevel ? (byte?)previousLevelCopy.LevelNr : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsLimitsForPeriodicityActive, (bool?)currentLevelCopy.IsLimitsForPeriodicityActive, !isCreatedLevel ? (bool?)previousLevelCopy.IsLimitsForPeriodicityActive : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevelLimitsPeriodicityValue, currentLevelCopy.LoyaltyLevelLimitsPeriodicityValue, !isCreatedLevel ? previousLevelCopy.LoyaltyLevelLimitsPeriodicityValue : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevelLimitsPeriodicity_UID, currentLevelCopy.LoyaltyLevelLimitsPeriodicity_UID, !isCreatedLevel ? previousLevelCopy.LoyaltyLevelLimitsPeriodicity_UID : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForNumberOfReservationsActive, (bool?)currentLevelCopy.IsForNumberOfReservationsActive, !isCreatedLevel ? (bool?)previousLevelCopy.IsForNumberOfReservationsActive : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForNumberOfReservationsValue, currentLevelCopy.IsForNumberOfReservationsValue, !isCreatedLevel ? previousLevelCopy.IsForNumberOfReservationsValue : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForNightsRoomActive, (bool?)currentLevelCopy.IsForNightsRoomActive, !isCreatedLevel ? (bool?)previousLevelCopy.IsForNightsRoomActive : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForNightsRoomValue, currentLevelCopy.IsForNightsRoomValue, !isCreatedLevel ? previousLevelCopy.IsForNightsRoomValue : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForTotalReservationsActive, (bool?)currentLevelCopy.IsForTotalReservationsActive, !isCreatedLevel ? (bool?)previousLevelCopy.IsForTotalReservationsActive : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForTotalReservationsValue, currentLevelCopy.IsForTotalReservationsValue, !isCreatedLevel ? previousLevelCopy.IsForTotalReservationsValue : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForReservationActive, (bool?)currentLevelCopy.IsForReservationActive, !isCreatedLevel ? (bool?)previousLevelCopy.IsForReservationActive : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForReservationRoomNightsValue, currentLevelCopy.IsForReservationRoomNightsValue, !isCreatedLevel ? previousLevelCopy.IsForReservationRoomNightsValue : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.IsForReservationValue, currentLevelCopy.IsForReservationValue, !isCreatedLevel ? previousLevelCopy.IsForReservationValue : null);
            levelDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.MigrateGuestsToLoyaltyLevelUID, isDeletedLevel ? migrateGuestsToLevelUID : null);

            var privilegedProperties = new List<contractsEvents.Enums.EntityPropertyEnum>()
            {
                contractsEvents.Enums.EntityPropertyEnum.Program_UID,
                contractsEvents.Enums.EntityPropertyEnum.Name,
                contractsEvents.Enums.EntityPropertyEnum.IsPercentage
            };

            // Keeps limit by period if one property changed
            if (currentLevelCopy.LoyaltyLevelLimitsPeriodicity_UID != previousLevelCopy.LoyaltyLevelLimitsPeriodicity_UID || currentLevelCopy.LoyaltyLevelLimitsPeriodicityValue != previousLevelCopy.LoyaltyLevelLimitsPeriodicityValue)
            {
                privilegedProperties.Add(contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevelLimitsPeriodicity_UID);
                privilegedProperties.Add(contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevelLimitsPeriodicityValue);
            }

            // Keeps discount value if isPercentageChanged
            if (currentLevelCopy.IsPercentage != previousLevelCopy.IsPercentage)
                privilegedProperties.Add(contractsEvents.Enums.EntityPropertyEnum.DiscountValue);

            // Add level delta to list
            return contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(levelDelta, privilegedProperties);
        }

        private List<contractsEvents.EntityDelta> CreateAssociatedDeltasOfLoyaltyLevel(contractsCRM.LoyaltyLevel currentLevel, contractsCRM.LoyaltyLevel previousLevel)
        {
            // If current level is null then consider equals to previous level
            var previousLevelCopy = IoHelper.Clone(previousLevel ?? new contractsCRM.LoyaltyLevel());
            var currentLevelCopy = IoHelper.Clone(currentLevel ?? previousLevel);
            var deltas = new List<contractsEvents.EntityDelta>();

            if (ratesUIDNamePropertyUID == null)
                ratesUIDNamePropertyUID = new Dictionary<long, Tuple<string,long>>();
            if (propertiesUIDName == null)
                propertiesUIDName = new Dictionary<long, string>();

            #region Create LoyaltyLevelLanguages Deltas

            var currentLevelLanguages = currentLevelCopy.LoyaltyLevelsLanguages ?? new List<contractsCRM.LoyaltyLevelsLanguage>();
            var previouLevelLanguages = previousLevelCopy.LoyaltyLevelsLanguages ?? new List<contractsCRM.LoyaltyLevelsLanguage>();

            // New or Modified LoyaltyLevelLanguages
            if (currentLevelLanguages.Any())
                foreach (var currentLevelLang in currentLevelLanguages)
                {
                    var previousLevelLang = previouLevelLanguages.FirstOrDefault(x => x.UID == currentLevelLang.UID);

                    var programLangState = contractsEvents.EntityState.Modified;
                    if (previousLevelLang == null)
                        programLangState = contractsEvents.EntityState.Created;

                    var levelLangDelta = this.CreateLoyaltyLevelLanguageDelta(programLangState, currentLevelLang, previousLevelLang);
                    deltas.Add(levelLangDelta); // Create delta
                }

            // Removed LoyaltyLevelLanguages
            if (previouLevelLanguages.Any())
                foreach (var previousLevelLang in previouLevelLanguages)
                {
                    var currentLevelLang = currentLevelLanguages.FirstOrDefault(x => x.UID == previousLevelLang.UID);

                    if (currentLevelLang != null)
                        continue;

                    var levelLangDelta = this.CreateLoyaltyLevelLanguageDelta(contractsEvents.EntityState.Deleted, currentLevelLang, previousLevelLang);
                    deltas.Add(levelLangDelta); // Create delta
                }

            #endregion

            #region Create LoyaltyLevelCurrencies Deltas

            var currentLevelCurrencies = currentLevelCopy.LoyaltyLevelsCurrencies ?? new List<contractsCRM.LoyaltyLevelsCurrency>();
            var previouLevelCurrencies = previousLevelCopy.LoyaltyLevelsCurrencies ?? new List<contractsCRM.LoyaltyLevelsCurrency>();

            // New or Modified LoyaltyLevelCurrencies
            if (currentLevelCurrencies.Any())
                foreach (var currentLevelCurr in currentLevelCurrencies)
                {
                    var previousLevelCurr = previouLevelCurrencies.FirstOrDefault(x => x.UID == currentLevelCurr.UID);

                    var programLangState = contractsEvents.EntityState.Modified;
                    if (previousLevelCurr == null)
                        programLangState = contractsEvents.EntityState.Created;

                    var levelCurrencyDelta = this.CreateLoyaltyLevelCurrencyDelta(programLangState, currentLevelCurr, previousLevelCurr);
                    deltas.Add(levelCurrencyDelta); // Create delta
                }

            // Removed LoyaltyLevelCurrencies
            if (previouLevelCurrencies.Any())
                foreach (var previousLevelCurr in previouLevelCurrencies)
                {
                    var currentLevelCurr = currentLevelCurrencies.FirstOrDefault(x => x.UID == previousLevelCurr.UID);

                    if (currentLevelCurr != null)
                        continue;

                    var levelCurrencyDelta = this.CreateLoyaltyLevelCurrencyDelta(contractsEvents.EntityState.Deleted, currentLevelCurr, previousLevelCurr);
                    deltas.Add(levelCurrencyDelta); // Create delta
                }

            #endregion

            #region Create RateLoyaltyLevels Deltas

            var currentRateLoyaltyLevels = currentLevelCopy.RateLoyaltyLevels ?? new List<contractsCRM.RateLoyaltyLevel>();
            var previouRateLoyaltyLevels = previousLevelCopy.RateLoyaltyLevels ?? new List<contractsCRM.RateLoyaltyLevel>();
            var rateLoyaltyLevelsDeltas = new List<contractsEvents.EntityDelta>();

            

            // New or Modified RateLoyaltyLevels
            if (currentRateLoyaltyLevels.Any())
                foreach (var currentRateLevel in currentRateLoyaltyLevels)
                {
                    var previousRateLevel = previouRateLoyaltyLevels.FirstOrDefault(x => x.LoyaltyLevel_UID == currentRateLevel.LoyaltyLevel_UID && x.Rate_UID == currentRateLevel.Rate_UID);

                    var programLangState = contractsEvents.EntityState.Modified;
                    if (previousRateLevel == null)
                        programLangState = contractsEvents.EntityState.Created;
                    
                    // Get rateName and property UID of rateLoyaltyLevel
                    Tuple<string, long> rateNamePropertyUID;
                    ratesUIDNamePropertyUID.TryGetValue(currentRateLevel.Rate_UID, out rateNamePropertyUID);
                    rateNamePropertyUID = rateNamePropertyUID ?? new Tuple<string, long>(null,0);

                    var rateLoyaltyLevelDelta = this.CreateRateLoyaltyLevelDelta(programLangState, currentRateLevel, previousRateLevel, rateNamePropertyUID.Item2, rateNamePropertyUID.Item1);
                    rateLoyaltyLevelsDeltas.Add(rateLoyaltyLevelDelta); // Create delta
                }

            // Removed RateLoyaltyLevels
            if (previouRateLoyaltyLevels.Any())
                foreach (var previousRateLevel in previouRateLoyaltyLevels)
                {
                    var currentRateLevel = currentRateLoyaltyLevels.FirstOrDefault(x => x.LoyaltyLevel_UID == previousRateLevel.LoyaltyLevel_UID && x.Rate_UID == previousRateLevel.Rate_UID);

                    if (currentRateLevel != null)
                        continue;

                    // Get rateName and property UID of rateLoyaltyLevel
                    Tuple<string, long> rateNamePropertyUID;
                    ratesUIDNamePropertyUID.TryGetValue(previousRateLevel.Rate_UID, out rateNamePropertyUID);
                    rateNamePropertyUID = rateNamePropertyUID ?? new Tuple<string, long>(null, 0);

                    var rateLoyaltyLevelDelta = this.CreateRateLoyaltyLevelDelta(contractsEvents.EntityState.Deleted, currentRateLevel, previousRateLevel, rateNamePropertyUID.Item2, rateNamePropertyUID.Item1);
                    rateLoyaltyLevelsDeltas.Add(rateLoyaltyLevelDelta); // Create delta
                }

            if (rateLoyaltyLevelsDeltas.Any())
                deltas.AddRange(rateLoyaltyLevelsDeltas);

            #endregion

            return deltas;
        }

        private contractsEvents.EntityDelta CreateLoyaltyLevelLanguageDelta(contractsEvents.EntityState entityState, contractsCRM.LoyaltyLevelsLanguage currentLevelLang, contractsCRM.LoyaltyLevelsLanguage previousLevelLang)
        {
            bool isDeleted = entityState == contractsEvents.EntityState.Deleted;
            bool isCreated = entityState == contractsEvents.EntityState.Created;

            var levelLanguageDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.LoyaltyLevelLanguage)
            {
                EntityKey = (entityState == contractsEvents.EntityState.Deleted) ? previousLevelLang.UID : currentLevelLang.UID,
                EntityState = entityState
            };

            levelLanguageDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevel_UID, !isDeleted ? (long?)currentLevelLang.LoyaltyLevel_UID : null, !isCreated ? (long?)previousLevelLang.LoyaltyLevel_UID : null);
            levelLanguageDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LanguageUID, !isDeleted ? (long?)currentLevelLang.Language_UID : null, !isCreated ? (long?)previousLevelLang.Language_UID : null);
            levelLanguageDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Name, !isDeleted ? currentLevelLang.Name : null, !isCreated ? previousLevelLang.Name : null);
            levelLanguageDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Description, !isDeleted ? currentLevelLang.Description : null, !isCreated ? previousLevelLang.Description : null);

            var privilegedProperties = new List<contractsEvents.Enums.EntityPropertyEnum>()
            {
                contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevel_UID,
                contractsEvents.Enums.EntityPropertyEnum.LanguageUID
            };
            return contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(levelLanguageDelta, privilegedProperties, true);
        }

        private contractsEvents.EntityDelta CreateLoyaltyLevelCurrencyDelta(contractsEvents.EntityState entityState, contractsCRM.LoyaltyLevelsCurrency currentLevelCurency, contractsCRM.LoyaltyLevelsCurrency previousLevelCurrency)
        {
            bool isDeleted = entityState == contractsEvents.EntityState.Deleted;
            bool isCreated = entityState == contractsEvents.EntityState.Created;

            var levelCurrencyDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.LoyaltyLevelCurrency)
            {
                EntityKey = (entityState == contractsEvents.EntityState.Deleted) ? previousLevelCurrency.UID : currentLevelCurency.UID,
                EntityState = entityState
            };

            // Get currency symbol
            string currencySymbol = this.AllCurrencies.Where(x => x.UID == (currentLevelCurency ?? previousLevelCurrency).Currency_UID).Select(x => x.Symbol).FirstOrDefault();

            levelCurrencyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevel_UID, !isDeleted ? (long?)currentLevelCurency.LoyaltyLevel_UID : null, !isCreated ? (long?)previousLevelCurrency.LoyaltyLevel_UID : null);
            levelCurrencyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Currency_UID, !isDeleted ? (long?)currentLevelCurency.Currency_UID : null, !isCreated ? (long?)previousLevelCurrency.Currency_UID : null);
            levelCurrencyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Value, !isDeleted ? (decimal?)currentLevelCurency.Value : null, !isCreated ? (decimal?)previousLevelCurrency.Value : null);
            levelCurrencyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.CurrencySymbol, !isDeleted ? currencySymbol : null, !isCreated ? currencySymbol : null);

            var privilegedProperties = new List<contractsEvents.Enums.EntityPropertyEnum>()
            {
                contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevel_UID,
                contractsEvents.Enums.EntityPropertyEnum.Currency_UID,
                contractsEvents.Enums.EntityPropertyEnum.CurrencySymbol
            };
            return contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(levelCurrencyDelta, privilegedProperties, true);
        }

        private contractsEvents.EntityDelta CreateRateLoyaltyLevelDelta(contractsEvents.EntityState entityState, contractsCRM.RateLoyaltyLevel currentRateLoyaltyLevel, contractsCRM.RateLoyaltyLevel previousRateLoyaltyLevel, long propertyUID, string rateName)
        {
            bool isDeleted = entityState == contractsEvents.EntityState.Deleted;
            bool isCreated = entityState == contractsEvents.EntityState.Created;

            var levelCurrencyDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.RateLoyaltyLevel)
            {
                EntityKey = (entityState == contractsEvents.EntityState.Deleted) ? previousRateLoyaltyLevel.UID : currentRateLoyaltyLevel.UID,
                EntityState = entityState
            };

            levelCurrencyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevel_UID, !isDeleted ? (long?)currentRateLoyaltyLevel.LoyaltyLevel_UID : null, !isCreated ? (long?)previousRateLoyaltyLevel.LoyaltyLevel_UID : null);
            levelCurrencyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.RateId, !isDeleted ? (long?)currentRateLoyaltyLevel.Rate_UID : null, !isCreated ? (long?)previousRateLoyaltyLevel.Rate_UID : null);
            levelCurrencyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.RateName, !isDeleted ? rateName : null, !isCreated ? rateName : null);
            levelCurrencyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.PropertyUID, !isDeleted ? (long?)propertyUID : null, !isCreated ? (long?)propertyUID : null);

            var privilegedProperties = new List<contractsEvents.Enums.EntityPropertyEnum>()
            {
                contractsEvents.Enums.EntityPropertyEnum.LoyaltyLevel_UID,
                contractsEvents.Enums.EntityPropertyEnum.RateId,
                contractsEvents.Enums.EntityPropertyEnum.RateName,
                contractsEvents.Enums.EntityPropertyEnum.PropertyUID
            };
            return contractsEvents.Helper.NotificationHelper.CleanDirtyChanges(levelCurrencyDelta, privilegedProperties, entityState == contractsEvents.EntityState.Modified);
        }

        private contractsEvents.EntityDelta CreatePropertyDelta(long propertyUID, string propertyName)
        {
            var propertyDelta = new contractsEvents.EntityDelta(contractsEvents.Enums.EntityEnum.Property)
            {
                EntityKey = propertyUID,
                EntityState = contractsEvents.EntityState.Unknown
            };

            propertyDelta.EntityProperties.AddProperty(contractsEvents.Enums.EntityPropertyEnum.Name, propertyName, propertyName);

            return propertyDelta;
        }

        private bool ProgramsAreEquals(contractsCRM.LoyaltyProgram currentProgram, contractsCRM.LoyaltyProgram previousProgram)
        {
            if ((currentProgram == null && previousProgram != null) || (currentProgram != null && previousProgram == null))
                return false;

            var copyOfCurrentProgram = IoHelper.Clone(currentProgram);
            var copyOfPreviousProgram = IoHelper.Clone(previousProgram);
            copyOfCurrentProgram.ModifiedBy = 0;
            copyOfPreviousProgram.ModifiedBy = 0;
            copyOfCurrentProgram.ModifiedByUsername = null;
            copyOfPreviousProgram.ModifiedByUsername = null;
            copyOfCurrentProgram.CreatedDate = default(DateTime);
            copyOfPreviousProgram.CreatedDate = default(DateTime);
            copyOfCurrentProgram.ModifiedDate = default(DateTime);
            copyOfPreviousProgram.ModifiedDate = default(DateTime);
            copyOfCurrentProgram.Revision = null;
            copyOfPreviousProgram.Revision = null;

            // Current Program Languages
            if (copyOfCurrentProgram.LoyaltyProgramsLanguages != null)
                foreach (var item in copyOfCurrentProgram.LoyaltyProgramsLanguages)
                    item.Revision = null;

            // Previous Program Languages
            if (copyOfPreviousProgram.LoyaltyProgramsLanguages != null)
                foreach (var item in copyOfPreviousProgram.LoyaltyProgramsLanguages)
                    item.Revision = null;

            // Current Program Levels
            if (copyOfCurrentProgram.LoyaltyLevels != null)
                foreach (var level in copyOfCurrentProgram.LoyaltyLevels)
                {
                    level.Revision = null;

                    if (level.LoyaltyLevelsLanguages != null)
                        foreach (var levelLang in level.LoyaltyLevelsLanguages)
                            levelLang.Revision = null;

                    if (level.LoyaltyLevelsCurrencies != null)
                        foreach (var levelCurr in level.LoyaltyLevelsCurrencies)
                            levelCurr.Revision = null;

                    if (level.RateLoyaltyLevels != null)
                        foreach (var rateLoyaltyLevel in level.RateLoyaltyLevels)
                            rateLoyaltyLevel.Revision = null;
                }

            // Previous Program Levels
            if (copyOfPreviousProgram.LoyaltyLevels != null)
                foreach (var level in copyOfPreviousProgram.LoyaltyLevels)
                {
                    level.Revision = null;

                    if (level.LoyaltyLevelsLanguages != null)
                        foreach (var levelLang in level.LoyaltyLevelsLanguages)
                            levelLang.Revision = null;

                    if (level.LoyaltyLevelsCurrencies != null)
                        foreach (var levelCurr in level.LoyaltyLevelsCurrencies)
                            levelCurr.Revision = null;

                    if (level.RateLoyaltyLevels != null)
                        foreach (var rateLoyaltyLevel in level.RateLoyaltyLevels)
                            rateLoyaltyLevel.Revision = null;
                }

            // It is deserialized two times because of decimal formats (e.g.: 2 is converted to 2.0 or 2.00)
            string currentProgramStr = Newtonsoft.Json.JsonConvert.SerializeObject(Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(copyOfCurrentProgram)));
            string previousProgramStr = Newtonsoft.Json.JsonConvert.SerializeObject(Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(copyOfPreviousProgram)));

            // Calculate MD5 hash of each object and compare them
            return Helper.CalculateMD5Hash.CalculateMD5(currentProgramStr) == Helper.CalculateMD5Hash.CalculateMD5(previousProgramStr);
        }

        private bool LevelsAreEquals(contractsCRM.LoyaltyLevel currentLevel, contractsCRM.LoyaltyLevel previousLevel)
        {
            if ((currentLevel == null && previousLevel != null) || (currentLevel != null && previousLevel == null))
                return false;

            var copyOfCurrentLevel = IoHelper.Clone(currentLevel);
            var copyOfPreviousLevel = IoHelper.Clone(previousLevel);

            copyOfCurrentLevel.Revision = null;
            copyOfPreviousLevel.Revision = null;

            if (copyOfCurrentLevel.LoyaltyLevelsLanguages != null)
                foreach (var levelLang in copyOfCurrentLevel.LoyaltyLevelsLanguages)
                    levelLang.Revision = null;

            if (copyOfPreviousLevel.LoyaltyLevelsLanguages != null)
                foreach (var levelLang in copyOfPreviousLevel.LoyaltyLevelsLanguages)
                    levelLang.Revision = null;

            if (copyOfCurrentLevel.LoyaltyLevelsCurrencies != null)
                foreach (var levelCurr in copyOfCurrentLevel.LoyaltyLevelsCurrencies)
                    levelCurr.Revision = null;

            if (copyOfPreviousLevel.LoyaltyLevelsCurrencies != null)
                foreach (var levelCurr in copyOfPreviousLevel.LoyaltyLevelsCurrencies)
                    levelCurr.Revision = null;

            if (copyOfCurrentLevel.RateLoyaltyLevels != null)
                foreach (var rateLoyaltyLevel in copyOfCurrentLevel.RateLoyaltyLevels)
                    rateLoyaltyLevel.Revision = null;

            if (copyOfPreviousLevel.RateLoyaltyLevels != null)
                foreach (var rateLoyaltyLevel in copyOfPreviousLevel.RateLoyaltyLevels)
                    rateLoyaltyLevel.Revision = null;

            // It is deserialized two times because of decimal formats (e.g.: 2 is converted to 2.0 or 2.00)
            string currentLevelStr = Newtonsoft.Json.JsonConvert.SerializeObject(Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(copyOfCurrentLevel)));
            string previousLevelStr = Newtonsoft.Json.JsonConvert.SerializeObject(Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(copyOfPreviousLevel)));

            // Calculate MD5 hash of each object and compare them
            return Helper.CalculateMD5Hash.CalculateMD5(currentLevelStr) == Helper.CalculateMD5Hash.CalculateMD5(previousLevelStr);
        }
    }
}
