import { notFound } from 'next/navigation';
import type { Metadata } from 'next';
import { getSite } from '../../lib/getSite';

export const metadata: Metadata = { title: 'Destek Ol' };

/**
 * ADR-024 §13: yalnız `DonationPageEnabled` açık ve en az bir banka hesabı varsa gösterilir.
 * Bu projede bayrak `false` başlar - ödeme/bağış akışı kapsam dışıdır (K-13), yalnız IBAN bilgisi.
 */
export default async function DonationPage() {
  const site = await getSite();
  if (!site.donationPageEnabled || site.bankAccounts.length === 0) notFound();

  return (
    <div className="container section content-detail">
      <h1>Destek Ol</h1>
      <p className="page-lead">Bağışlarınızı aşağıdaki banka hesaplarına yapabilirsiniz.</p>
      <ul className="bank-account-list">
        {site.bankAccounts.map((account, index) => (
          <li key={index} className="bank-account-list__item">
            <p className="bank-account-list__bank">{account.bankName}</p>
            <p className="bank-account-list__iban">{account.iban}</p>
            <p>{account.accountHolder}</p>
            {account.description && <p className="field-hint">{account.description}</p>}
          </li>
        ))}
      </ul>
    </div>
  );
}
