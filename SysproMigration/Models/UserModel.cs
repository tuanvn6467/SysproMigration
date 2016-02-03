using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SysproMigration.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public int? TenantID { get; set; }
        public Guid? UserGUID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public short? StatusID { get; set; }
        public short? LicenseType { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public string WorkPhone { get; set; }
        public string WorkPhoneExt { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string FaxPhone { get; set; }
        public short? MailingAddress { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public long County { get; set; }
        public long State { get; set; }
        public string PostalCode { get; set; }
        public long Country { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? CreatedUserID { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedUserID { get; set; }

        public string FullName { get; set; }

        /*** modify missing fields***/
        public string TimeZone { get; set; }
        public short IsRegionalSettings { get; set; }
        public short DateFormatType { get; set; }
        public string DatePictureFormat { get; set; }
        public short TimeFormatType { get; set; }
        public string TimePictureFormat { get; set; }
        public string PhoneNumberFormat { get; set; }
        public short IsUsePostalCodeTable { get; set; }
        public string DialFromLocation { get; set; }
        public string DefaultTemporaryDirectory { get; set; }
        public int? DefaultCalendarID { get; set; }
        public string HighlightColor { get; set; }
        public string BackgroundColor { get; set; }
        public short IsMultiPhone { get; set; }
        public int? DefaultActivityTypeID { get; set; }
        public int? DefaultActivityLocationID { get; set; }
        public int? DefaultAppointmentTypeID { get; set; }
        public int DefaultTaskPriorityID { get; set; }
        public int DefaultEmailMethod { get; set; }
        public int DefaultFaxMethod { get; set; }
        public int ExcludePNGEmbedded { get; set; }
        public int DefaultEmailAccount { get; set; }
        public short IsOutlookSyncInboundEmails { get; set; }
        public short IsEmailOptionReadFirst { get; set; }
        public short IsEmailOptionAllow { get; set; }
        public short IsOutlookAutoSyncFlag { get; set; }
        public short IsOutlookSyncOnLogin { get; set; }
        public short IsOutlookSynchronizeAppointment { get; set; }
        public short IsWriteAppointmentsToOutlook { get; set; }
        public short IsOutlookSynchronizeContact { get; set; }
        public short IsWriteContactsToOutlook { get; set; }
        public short IsOutlookSynchronizeTask { get; set; }
        public short WriteTasksToOutlook { get; set; }
        public int OutlookAutoSyncMinutes { get; set; }
        public short IsEnableAlarm { get; set; }
        public short IsAlarmOnNewAppt { get; set; }
        public short AlarmOnApptDuePeriod { get; set; }
        public short AlarmOnApptDueUnits { get; set; }
        public short AlarmOnNewApptPriority { get; set; }
        public short IsAlarmOnNewTask { get; set; }
        public short AlarmOnTaskDuePeriod { get; set; }
        public short AlarmOnTaskDueUnits { get; set; }
        public short AlarmOnNewTaskPriority { get; set; }
        public short DefaultSnoozePeriod { get; set; }
        public short DefaultSnoozeUnits { get; set; }
        public short IsAdministrator { get; set; }
        public long? HomeCountry { get; set; }
        public short IsOutlookContactPreference { get; set; }
    }
}