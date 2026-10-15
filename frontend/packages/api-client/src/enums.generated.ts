/**
 * ÜRETİLDİ: elle düzenlemeyin.
 * Kaynak: docs/api/enums.v1.json (`pnpm api:gen` ile üretilir).
 * Enum kod listesinin TEK kaynağı budur; başka yerde enum üye adları elle yazılmaz.
 */

/** Kaynak: Identity.AdminActionType (docs/api/enums.v1.json). */
export const AdminActionType = {
  Deactivated: "Deactivated",
  ManuallyUnlocked: "ManuallyUnlocked",
  Reactivated: "Reactivated",
  RoleChanged: "RoleChanged",
} as const;
export type AdminActionType = (typeof AdminActionType)[keyof typeof AdminActionType];

/** Kaynak: Identity.UserRole (docs/api/enums.v1.json). */
export const UserRole = {
  Admin: "Admin",
  Candidate: "Candidate",
  CareerAdvisor: "CareerAdvisor",
  Employer: "Employer",
} as const;
export type UserRole = (typeof UserRole)[keyof typeof UserRole];

/** Kaynak: Identity.UserStatus (docs/api/enums.v1.json). */
export const UserStatus = {
  Active: "Active",
  Deleted: "Deleted",
  Disabled: "Disabled",
  Locked: "Locked",
  Pending: "Pending",
  Suspended: "Suspended",
} as const;
export type UserStatus = (typeof UserStatus)[keyof typeof UserStatus];

/** Kaynak: Website.BankAccountCurrency (docs/api/enums.v1.json). */
export const BankAccountCurrency = {
  EUR: "EUR",
  TRY: "TRY",
  USD: "USD",
} as const;
export type BankAccountCurrency = (typeof BankAccountCurrency)[keyof typeof BankAccountCurrency];

/** Kaynak: Website.ContentItemRevisionKind (docs/api/enums.v1.json). */
export const ContentItemRevisionKind = {
  Created: "Created",
  Edited: "Edited",
  Published: "Published",
  Restored: "Restored",
} as const;
export type ContentItemRevisionKind = (typeof ContentItemRevisionKind)[keyof typeof ContentItemRevisionKind];

/** Kaynak: Website.ContentItemStatus (docs/api/enums.v1.json). */
export const ContentItemStatus = {
  Archived: "Archived",
  Draft: "Draft",
  Published: "Published",
  Unpublished: "Unpublished",
} as const;
export type ContentItemStatus = (typeof ContentItemStatus)[keyof typeof ContentItemStatus];

/** Kaynak: Website.ContentSchemaKind (docs/api/enums.v1.json). */
export const ContentSchemaKind = {
  Article: "Article",
  FaqPage: "FaqPage",
  NewsArticle: "NewsArticle",
  None: "None",
} as const;
export type ContentSchemaKind = (typeof ContentSchemaKind)[keyof typeof ContentSchemaKind];

/** Kaynak: Website.ContentTypeSortMode (docs/api/enums.v1.json). */
export const ContentTypeSortMode = {
  EventDateAsc: "EventDateAsc",
  Manual: "Manual",
  PublishDateDesc: "PublishDateDesc",
} as const;
export type ContentTypeSortMode = (typeof ContentTypeSortMode)[keyof typeof ContentTypeSortMode];

/** Kaynak: Website.CookieConsentAction (docs/api/enums.v1.json). */
export const CookieConsentAction = {
  AcceptAll: "AcceptAll",
  Custom: "Custom",
  RejectAll: "RejectAll",
} as const;
export type CookieConsentAction = (typeof CookieConsentAction)[keyof typeof CookieConsentAction];

/** Kaynak: Website.EventCapacityDecision (docs/api/enums.v1.json). */
export const EventCapacityDecision = {
  Applied: "Applied",
  Confirmed: "Confirmed",
  Waitlisted: "Waitlisted",
} as const;
export type EventCapacityDecision = (typeof EventCapacityDecision)[keyof typeof EventCapacityDecision];

/** Kaynak: Website.EventFormat (docs/api/enums.v1.json). */
export const EventFormat = {
  Hybrid: "Hybrid",
  InPerson: "InPerson",
  Online: "Online",
} as const;
export type EventFormat = (typeof EventFormat)[keyof typeof EventFormat];

/** Kaynak: Website.EventRegistrationCancelOutcome (docs/api/enums.v1.json). */
export const EventRegistrationCancelOutcome = {
  AlreadyCancelled: "AlreadyCancelled",
  ReleasedConfirmedSlot: "ReleasedConfirmedSlot",
  ReleasedNoSlot: "ReleasedNoSlot",
  ReleasedWaitlistSlot: "ReleasedWaitlistSlot",
} as const;
export type EventRegistrationCancelOutcome = (typeof EventRegistrationCancelOutcome)[keyof typeof EventRegistrationCancelOutcome];

/** Kaynak: Website.EventRegistrationCancelledBy (docs/api/enums.v1.json). */
export const EventRegistrationCancelledBy = {
  Admin: "Admin",
  Event: "Event",
  Participant: "Participant",
} as const;
export type EventRegistrationCancelledBy = (typeof EventRegistrationCancelledBy)[keyof typeof EventRegistrationCancelledBy];

/** Kaynak: Website.EventRegistrationState (docs/api/enums.v1.json). */
export const EventRegistrationState = {
  Cancelled: "Cancelled",
  Closed: "Closed",
  Full: "Full",
  NotOpen: "NotOpen",
  Open: "Open",
  WaitlistOpen: "WaitlistOpen",
} as const;
export type EventRegistrationState = (typeof EventRegistrationState)[keyof typeof EventRegistrationState];

/** Kaynak: Website.EventRegistrationStatus (docs/api/enums.v1.json). */
export const EventRegistrationStatus = {
  Applied: "Applied",
  Attended: "Attended",
  Cancelled: "Cancelled",
  Confirmed: "Confirmed",
  NoShow: "NoShow",
  PendingVerification: "PendingVerification",
  Rejected: "Rejected",
  Waitlisted: "Waitlisted",
} as const;
export type EventRegistrationStatus = (typeof EventRegistrationStatus)[keyof typeof EventRegistrationStatus];

/** Kaynak: Website.EventTimeWindow (docs/api/enums.v1.json). */
export const EventTimeWindow = {
  All: "All",
  Past: "Past",
  Upcoming: "Upcoming",
} as const;
export type EventTimeWindow = (typeof EventTimeWindow)[keyof typeof EventTimeWindow];

/** Kaynak: Website.FormFieldAllowedFileType (docs/api/enums.v1.json). */
export const FormFieldAllowedFileType = {
  Docx: "Docx",
  Jpg: "Jpg",
  Pdf: "Pdf",
  Png: "Png",
} as const;
export type FormFieldAllowedFileType = (typeof FormFieldAllowedFileType)[keyof typeof FormFieldAllowedFileType];

/** Kaynak: Website.FormFieldType (docs/api/enums.v1.json). */
export const FormFieldType = {
  Checkbox: "Checkbox",
  Date: "Date",
  Email: "Email",
  File: "File",
  MultiSelect: "MultiSelect",
  Phone: "Phone",
  Select: "Select",
  Text: "Text",
  Textarea: "Textarea",
} as const;
export type FormFieldType = (typeof FormFieldType)[keyof typeof FormFieldType];

/** Kaynak: Website.FormSubmissionStatus (docs/api/enums.v1.json). */
export const FormSubmissionStatus = {
  Approved: "Approved",
  AwaitingInfo: "AwaitingInfo",
  Completed: "Completed",
  InReview: "InReview",
  New: "New",
  Rejected: "Rejected",
} as const;
export type FormSubmissionStatus = (typeof FormSubmissionStatus)[keyof typeof FormSubmissionStatus];

/** Kaynak: Website.LegalDocumentKind (docs/api/enums.v1.json). */
export const LegalDocumentKind = {
  CookiePolicy: "CookiePolicy",
  ExplicitConsent: "ExplicitConsent",
  Other: "Other",
  PrivacyNotice: "PrivacyNotice",
  TermsOfUse: "TermsOfUse",
} as const;
export type LegalDocumentKind = (typeof LegalDocumentKind)[keyof typeof LegalDocumentKind];

/** Kaynak: Website.LegalDocumentVersionStatus (docs/api/enums.v1.json). */
export const LegalDocumentVersionStatus = {
  Draft: "Draft",
  Published: "Published",
  Superseded: "Superseded",
} as const;
export type LegalDocumentVersionStatus = (typeof LegalDocumentVersionStatus)[keyof typeof LegalDocumentVersionStatus];

/** Kaynak: Website.LinkTargetKind (docs/api/enums.v1.json). */
export const LinkTargetKind = {
  Content: "Content",
  ContentTypeListing: "ContentTypeListing",
  ExternalUrl: "ExternalUrl",
  InternalPath: "InternalPath",
  None: "None",
} as const;
export type LinkTargetKind = (typeof LinkTargetKind)[keyof typeof LinkTargetKind];

/** Kaynak: Website.MediaAssetKind (docs/api/enums.v1.json). */
export const MediaAssetKind = {
  Document: "Document",
  Image: "Image",
} as const;
export type MediaAssetKind = (typeof MediaAssetKind)[keyof typeof MediaAssetKind];

/** Kaynak: Website.MenuLocation (docs/api/enums.v1.json). */
export const MenuLocation = {
  Footer: "Footer",
  Header: "Header",
  Utility: "Utility",
} as const;
export type MenuLocation = (typeof MenuLocation)[keyof typeof MenuLocation];

/** Kaynak: Website.NewsletterSubscriberStatus (docs/api/enums.v1.json). */
export const NewsletterSubscriberStatus = {
  Active: "Active",
  PendingConfirmation: "PendingConfirmation",
  Unsubscribed: "Unsubscribed",
} as const;
export type NewsletterSubscriberStatus = (typeof NewsletterSubscriberStatus)[keyof typeof NewsletterSubscriberStatus];

/** Kaynak: Website.PageLayoutTargetKind (docs/api/enums.v1.json). */
export const PageLayoutTargetKind = {
  Content: "Content",
  Home: "Home",
} as const;
export type PageLayoutTargetKind = (typeof PageLayoutTargetKind)[keyof typeof PageLayoutTargetKind];

/** Kaynak: Website.PersonalDataAccessAction (docs/api/enums.v1.json). */
export const PersonalDataAccessAction = {
  DownloadFile: "DownloadFile",
  Export: "Export",
  View: "View",
} as const;
export type PersonalDataAccessAction = (typeof PersonalDataAccessAction)[keyof typeof PersonalDataAccessAction];

/** Kaynak: Website.PersonalDataEntityType (docs/api/enums.v1.json). */
export const PersonalDataEntityType = {
  EventRegistration: "EventRegistration",
  FormSubmission: "FormSubmission",
  NewsletterSubscriber: "NewsletterSubscriber",
} as const;
export type PersonalDataEntityType = (typeof PersonalDataEntityType)[keyof typeof PersonalDataEntityType];

/** Kaynak: Website.PopupDeviceTarget (docs/api/enums.v1.json). */
export const PopupDeviceTarget = {
  All: "All",
  Desktop: "Desktop",
  Mobile: "Mobile",
} as const;
export type PopupDeviceTarget = (typeof PopupDeviceTarget)[keyof typeof PopupDeviceTarget];

/** Kaynak: Website.PopupDisplayMode (docs/api/enums.v1.json). */
export const PopupDisplayMode = {
  Banner: "Banner",
  Modal: "Modal",
} as const;
export type PopupDisplayMode = (typeof PopupDisplayMode)[keyof typeof PopupDisplayMode];

/** Kaynak: Website.PopupFrequency (docs/api/enums.v1.json). */
export const PopupFrequency = {
  EveryNDays: "EveryNDays",
  EveryVisit: "EveryVisit",
  OncePerSession: "OncePerSession",
} as const;
export type PopupFrequency = (typeof PopupFrequency)[keyof typeof PopupFrequency];

/** Kaynak: Website.PopupTargetingKind (docs/api/enums.v1.json). */
export const PopupTargetingKind = {
  AllPages: "AllPages",
  Contents: "Contents",
  HomeOnly: "HomeOnly",
  Paths: "Paths",
} as const;
export type PopupTargetingKind = (typeof PopupTargetingKind)[keyof typeof PopupTargetingKind];

/** Kaynak: Website.RedirectStatusCode (docs/api/enums.v1.json). */
export const RedirectStatusCode = {
  Found: "Found",
  MovedPermanently: "MovedPermanently",
} as const;
export type RedirectStatusCode = (typeof RedirectStatusCode)[keyof typeof RedirectStatusCode];

/** Kaynak: Website.RedirectTargetKind (docs/api/enums.v1.json). */
export const RedirectTargetKind = {
  ContentItem: "ContentItem",
  Path: "Path",
} as const;
export type RedirectTargetKind = (typeof RedirectTargetKind)[keyof typeof RedirectTargetKind];

/** Kaynak: Website.RouteResolutionKind (docs/api/enums.v1.json). */
export const RouteResolutionKind = {
  Detail: "Detail",
  Home: "Home",
  Listing: "Listing",
  NotFound: "NotFound",
  Redirect: "Redirect",
} as const;
export type RouteResolutionKind = (typeof RouteResolutionKind)[keyof typeof RouteResolutionKind];

/** Kaynak: Website.ThirdPartyScriptCategory (docs/api/enums.v1.json). */
export const ThirdPartyScriptCategory = {
  Analytics: "Analytics",
  Marketing: "Marketing",
  Necessary: "Necessary",
} as const;
export type ThirdPartyScriptCategory = (typeof ThirdPartyScriptCategory)[keyof typeof ThirdPartyScriptCategory];

/** Kaynak: Website.ThirdPartyScriptPlacement (docs/api/enums.v1.json). */
export const ThirdPartyScriptPlacement = {
  BodyEnd: "BodyEnd",
  Head: "Head",
} as const;
export type ThirdPartyScriptPlacement = (typeof ThirdPartyScriptPlacement)[keyof typeof ThirdPartyScriptPlacement];

/** Kaynak: Website.ThirdPartyScriptProviderKind (docs/api/enums.v1.json). */
export const ThirdPartyScriptProviderKind = {
  ExternalScript: "ExternalScript",
  GoogleAnalytics4: "GoogleAnalytics4",
  GoogleTagManager: "GoogleTagManager",
  MetaPixel: "MetaPixel",
} as const;
export type ThirdPartyScriptProviderKind = (typeof ThirdPartyScriptProviderKind)[keyof typeof ThirdPartyScriptProviderKind];

export const ENUM_NUMERIC_CODES = {
  AdminActionType: {
    Deactivated: 2,
    ManuallyUnlocked: 1,
    Reactivated: 3,
    RoleChanged: 0,
  },
  UserRole: {
    Admin: 0,
    Candidate: 2,
    CareerAdvisor: 3,
    Employer: 1,
  },
  UserStatus: {
    Active: 1,
    Deleted: 5,
    Disabled: 4,
    Locked: 3,
    Pending: 0,
    Suspended: 2,
  },
  BankAccountCurrency: {
    EUR: 2,
    TRY: 0,
    USD: 1,
  },
  ContentItemRevisionKind: {
    Created: 0,
    Edited: 1,
    Published: 2,
    Restored: 3,
  },
  ContentItemStatus: {
    Archived: 3,
    Draft: 0,
    Published: 1,
    Unpublished: 2,
  },
  ContentSchemaKind: {
    Article: 1,
    FaqPage: 3,
    NewsArticle: 2,
    None: 0,
  },
  ContentTypeSortMode: {
    EventDateAsc: 2,
    Manual: 0,
    PublishDateDesc: 1,
  },
  CookieConsentAction: {
    AcceptAll: 0,
    Custom: 2,
    RejectAll: 1,
  },
  EventCapacityDecision: {
    Applied: 0,
    Confirmed: 1,
    Waitlisted: 2,
  },
  EventFormat: {
    Hybrid: 2,
    InPerson: 0,
    Online: 1,
  },
  EventRegistrationCancelOutcome: {
    AlreadyCancelled: 0,
    ReleasedConfirmedSlot: 1,
    ReleasedNoSlot: 3,
    ReleasedWaitlistSlot: 2,
  },
  EventRegistrationCancelledBy: {
    Admin: 1,
    Event: 2,
    Participant: 0,
  },
  EventRegistrationState: {
    Cancelled: 5,
    Closed: 4,
    Full: 2,
    NotOpen: 0,
    Open: 1,
    WaitlistOpen: 3,
  },
  EventRegistrationStatus: {
    Applied: 1,
    Attended: 6,
    Cancelled: 5,
    Confirmed: 2,
    NoShow: 7,
    PendingVerification: 0,
    Rejected: 4,
    Waitlisted: 3,
  },
  EventTimeWindow: {
    All: 2,
    Past: 1,
    Upcoming: 0,
  },
  FormFieldAllowedFileType: {
    Docx: 1,
    Jpg: 2,
    Pdf: 0,
    Png: 3,
  },
  FormFieldType: {
    Checkbox: 6,
    Date: 7,
    Email: 1,
    File: 8,
    MultiSelect: 5,
    Phone: 2,
    Select: 4,
    Text: 0,
    Textarea: 3,
  },
  FormSubmissionStatus: {
    Approved: 3,
    AwaitingInfo: 2,
    Completed: 5,
    InReview: 1,
    New: 0,
    Rejected: 4,
  },
  LegalDocumentKind: {
    CookiePolicy: 2,
    ExplicitConsent: 1,
    Other: 4,
    PrivacyNotice: 0,
    TermsOfUse: 3,
  },
  LegalDocumentVersionStatus: {
    Draft: 0,
    Published: 1,
    Superseded: 2,
  },
  LinkTargetKind: {
    Content: 1,
    ContentTypeListing: 2,
    ExternalUrl: 4,
    InternalPath: 3,
    None: 0,
  },
  MediaAssetKind: {
    Document: 1,
    Image: 0,
  },
  MenuLocation: {
    Footer: 2,
    Header: 0,
    Utility: 1,
  },
  NewsletterSubscriberStatus: {
    Active: 1,
    PendingConfirmation: 0,
    Unsubscribed: 2,
  },
  PageLayoutTargetKind: {
    Content: 1,
    Home: 0,
  },
  PersonalDataAccessAction: {
    DownloadFile: 1,
    Export: 2,
    View: 0,
  },
  PersonalDataEntityType: {
    EventRegistration: 2,
    FormSubmission: 0,
    NewsletterSubscriber: 1,
  },
  PopupDeviceTarget: {
    All: 0,
    Desktop: 1,
    Mobile: 2,
  },
  PopupDisplayMode: {
    Banner: 1,
    Modal: 0,
  },
  PopupFrequency: {
    EveryNDays: 2,
    EveryVisit: 0,
    OncePerSession: 1,
  },
  PopupTargetingKind: {
    AllPages: 0,
    Contents: 2,
    HomeOnly: 1,
    Paths: 3,
  },
  RedirectStatusCode: {
    Found: 302,
    MovedPermanently: 301,
  },
  RedirectTargetKind: {
    ContentItem: 0,
    Path: 1,
  },
  RouteResolutionKind: {
    Detail: 1,
    Home: 0,
    Listing: 2,
    NotFound: 4,
    Redirect: 3,
  },
  ThirdPartyScriptCategory: {
    Analytics: 1,
    Marketing: 2,
    Necessary: 0,
  },
  ThirdPartyScriptPlacement: {
    BodyEnd: 1,
    Head: 0,
  },
  ThirdPartyScriptProviderKind: {
    ExternalScript: 3,
    GoogleAnalytics4: 0,
    GoogleTagManager: 1,
    MetaPixel: 2,
  },
} as const;

export type EnumName = keyof typeof ENUM_NUMERIC_CODES;
