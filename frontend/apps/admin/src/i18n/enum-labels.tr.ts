import { ENUM_NUMERIC_CODES } from '@gm/api-client';

/**
 * Her sistem enum'u için Türkçe etiket. `satisfies` ile `ENUM_NUMERIC_CODES`'a (üretilmiş,
 * `docs/api/enums.v1.json`'dan) karşı tam kapsama zorunludur: manifeste enum/üye eklenip bu
 * sözlük güncellenmezse `pnpm api:gen` + `pnpm typecheck` sonrası DERLEME HATASI verir.
 * Yeni dil eklemek için aynı şekle sahip `enum-labels.<dil>.ts` oluşturup `getEnumLabel.ts`'e
 * kaydedin; başka hiçbir yerde ham enum üye adı arayüzde gösterilmez.
 */
type EnumLabelDictionary = {
  [EnumKey in keyof typeof ENUM_NUMERIC_CODES]: {
    [Member in keyof (typeof ENUM_NUMERIC_CODES)[EnumKey]]: string;
  };
};

export const trEnumLabels = {
  UserRole: {
    Admin: 'Yönetici',
    Candidate: 'Aday',
    CareerAdvisor: 'Kariyer danışmanı',
    Employer: 'İşveren',
  },
  UserStatus: {
    Pending: 'Beklemede',
    Active: 'Aktif',
    Suspended: 'Askıya alınmış',
    Locked: 'Kilitli',
    Disabled: 'Devre dışı',
    Deleted: 'Silinmiş',
  },
  AdminActionType: {
    RoleChanged: 'Rol değiştirildi',
    ManuallyUnlocked: 'Elle kilit açıldı',
    Deactivated: 'Devre dışı bırakıldı',
    Reactivated: 'Yeniden etkinleştirildi',
  },
  BankAccountCurrency: {
    TRY: 'Türk lirası',
    USD: 'Amerikan doları',
    EUR: 'Euro',
  },
  ContentItemRevisionKind: {
    Created: 'Oluşturuldu',
    Edited: 'Düzenlendi',
    Published: 'Yayınlandı',
    Restored: 'Geri yüklendi',
  },
  ContentItemStatus: {
    Draft: 'Taslak',
    Published: 'Yayında',
    Unpublished: 'Yayından kaldırıldı',
    Archived: 'Arşivlendi',
  },
  ContentSchemaKind: {
    None: 'Yok',
    Article: 'Makale',
    NewsArticle: 'Haber',
    FaqPage: 'Sık sorulan sorular',
  },
  ContentTypeSortMode: {
    Manual: 'Elle sıralama',
    PublishDateDesc: 'Yayın tarihi (yeni → eski)',
    EventDateAsc: 'Etkinlik tarihi (eski → yeni)',
  },
  CookieConsentAction: {
    AcceptAll: 'Tümünü kabul et',
    RejectAll: 'Tümünü reddet',
    Custom: 'Özelleştir',
  },
  EventCapacityDecision: {
    Applied: 'Başvuruldu',
    Confirmed: 'Onaylandı',
    Waitlisted: 'Bekleme listesine alındı',
  },
  EventFormat: {
    InPerson: 'Yüz yüze',
    Online: 'Çevrim içi',
    Hybrid: 'Hibrit',
  },
  EventRegistrationCancelOutcome: {
    AlreadyCancelled: 'Zaten iptal edilmiş',
    ReleasedConfirmedSlot: 'Onaylı yer boşaltıldı',
    ReleasedWaitlistSlot: 'Bekleme listesi yeri boşaltıldı',
    ReleasedNoSlot: 'Yer boşaltılmadı',
  },
  EventRegistrationCancelledBy: {
    Participant: 'Katılımcı',
    Admin: 'Yönetici',
    Event: 'Etkinlik',
  },
  EventRegistrationState: {
    NotOpen: 'Henüz açılmadı',
    Open: 'Açık',
    Full: 'Dolu',
    WaitlistOpen: 'Bekleme listesi açık',
    Closed: 'Kapandı',
    Cancelled: 'İptal edildi',
  },
  EventRegistrationStatus: {
    PendingVerification: 'Doğrulama bekleniyor',
    Applied: 'Başvuruldu',
    Confirmed: 'Onaylandı',
    Waitlisted: 'Bekleme listesinde',
    Rejected: 'Reddedildi',
    Cancelled: 'İptal edildi',
    Attended: 'Katıldı',
    NoShow: 'Gelmedi',
  },
  EventTimeWindow: {
    Upcoming: 'Yaklaşan',
    Past: 'Geçmiş',
    All: 'Tümü',
  },
  FormFieldAllowedFileType: {
    Pdf: 'PDF',
    Docx: 'Word belgesi (.docx)',
    Jpg: 'JPEG görseli',
    Png: 'PNG görseli',
  },
  FormFieldType: {
    Text: 'Metin',
    Email: 'E-posta',
    Phone: 'Telefon',
    Textarea: 'Uzun metin',
    Select: 'Seçim listesi',
    MultiSelect: 'Çoklu seçim',
    Checkbox: 'Onay kutusu',
    Date: 'Tarih',
    File: 'Dosya',
  },
  FormSubmissionStatus: {
    New: 'Yeni',
    InReview: 'İnceleniyor',
    AwaitingInfo: 'Bilgi bekleniyor',
    Approved: 'Onaylandı',
    Rejected: 'Reddedildi',
    Completed: 'Tamamlandı',
  },
  LegalDocumentKind: {
    PrivacyNotice: 'Gizlilik bildirimi',
    ExplicitConsent: 'Açık rıza metni',
    CookiePolicy: 'Çerez politikası',
    TermsOfUse: 'Kullanım şartları',
    Other: 'Diğer',
  },
  LegalDocumentVersionStatus: {
    Draft: 'Taslak',
    Published: 'Yayında',
    Superseded: 'Yürürlükten kaldırıldı',
  },
  LinkTargetKind: {
    None: 'Yok',
    Content: 'İçerik',
    ContentTypeListing: 'İçerik tipi listesi',
    InternalPath: 'İç yol',
    ExternalUrl: 'Dış bağlantı',
  },
  MediaAssetKind: {
    Image: 'Görsel',
    Document: 'Belge',
  },
  MenuLocation: {
    Header: 'Üst menü',
    Utility: 'Yardımcı menü',
    Footer: 'Alt bilgi (footer)',
  },
  NewsletterSubscriberStatus: {
    PendingConfirmation: 'Onay bekleniyor',
    Active: 'Aktif',
    Unsubscribed: 'Abonelikten çıkıldı',
  },
  PageLayoutTargetKind: {
    Home: 'Ana sayfa',
    Content: 'İçerik sayfası',
  },
  PersonalDataAccessAction: {
    View: 'Görüntülendi',
    DownloadFile: 'Dosya indirildi',
    Export: 'Dışa aktarıldı',
  },
  PersonalDataEntityType: {
    FormSubmission: 'Form gönderimi',
    NewsletterSubscriber: 'Bülten aboneliği',
    EventRegistration: 'Etkinlik kaydı',
  },
  PopupDeviceTarget: {
    All: 'Tümü',
    Desktop: 'Masaüstü',
    Mobile: 'Mobil',
  },
  PopupDisplayMode: {
    Modal: 'Pop-up (modal)',
    Banner: 'Banner',
  },
  PopupFrequency: {
    EveryVisit: 'Her ziyarette',
    OncePerSession: 'Oturumda bir kez',
    EveryNDays: 'Her N günde bir',
  },
  PopupTargetingKind: {
    AllPages: 'Tüm sayfalar',
    HomeOnly: 'Yalnız ana sayfa',
    Contents: 'Belirli içerikler',
    Paths: 'Belirli yollar',
  },
  RedirectStatusCode: {
    MovedPermanently: '301 - Kalıcı taşındı',
    Found: '302 - Geçici yönlendirme',
  },
  RedirectTargetKind: {
    ContentItem: 'İçerik',
    Path: 'Yol',
  },
  RouteResolutionKind: {
    Home: 'Ana sayfa',
    Listing: 'Liste',
    Detail: 'Detay',
    Redirect: 'Yönlendirme',
    NotFound: 'Bulunamadı',
  },
  ThirdPartyScriptCategory: {
    Necessary: 'Gerekli',
    Analytics: 'Analitik',
    Marketing: 'Pazarlama',
  },
  ThirdPartyScriptPlacement: {
    Head: 'Head (üst bilgi)',
    BodyEnd: 'Gövde sonu',
  },
  ThirdPartyScriptProviderKind: {
    GoogleAnalytics4: 'Google Analytics 4',
    GoogleTagManager: 'Google Tag Manager',
    MetaPixel: 'Meta Pixel',
    ExternalScript: 'Dış script',
  },
} satisfies EnumLabelDictionary;
