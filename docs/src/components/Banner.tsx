import useLocalStorage from '@rehooks/local-storage';
import { XMarkIcon } from '@heroicons/react/20/solid';

export default function Banner({
  title,
  bannerKey,
  description,
  cta,
  ctaLink,
  changelogLink,
  changelogTitle,
}: {
  title: string;
  bannerKey: string;
  description: string;
  cta: string;
  ctaLink: string;
  changelogLink: string;
  changelogTitle: string;
}) {
  const [dismissed, setDismissed] = useLocalStorage('bannerDismissed_' + bannerKey, false);

  if (dismissed) return null;

  return (
    <div className="relative isolate flex items-center gap-x-6 overflow-hidden bg-sky-900 px-6 py-2.5 sm:px-3.5 sm:before:flex-1">
      <div
        className="absolute left-[max(-7rem,calc(50%-52rem))] top-1/2 -z-10 -translate-y-1/2 transform-gpu blur-2xl"
        aria-hidden="true"
      >
        <div
          className="aspect-[577/310] w-[36.0625rem] animate-pulse duration-8s bg-gradient-to-r from-orange-300 to-purple-500 opacity-100"
          style={{
            clipPath:
              'polygon(74.8% 41.9%, 97.2% 73.2%, 100% 34.9%, 92.5% 0.4%, 87.5% 0%, 75% 28.6%, 58.5% 54.6%, 50.1% 56.8%, 46.9% 44%, 48.3% 17.4%, 24.7% 53.9%, 0% 27.9%, 11.9% 74.2%, 24.9% 54.1%, 68.6% 100%, 74.8% 41.9%)',
          }}
        />
      </div>
      <div
        className="absolute left-[max(45rem,calc(50%+8rem))] top-1/2 -z-10 -translate-y-1/2 transform-gpu blur-2xl"
        aria-hidden="true"
      >
        <div
          className="aspect-[577/310] w-[36.0625rem] animate-pulse duration-8s delay-500 bg-gradient-to-r from-orange-300 to-purple-500 opacity-100"
          style={{
            clipPath:
              'polygon(74.8% 41.9%, 97.2% 73.2%, 100% 34.9%, 92.5% 0.4%, 87.5% 0%, 75% 28.6%, 58.5% 54.6%, 50.1% 56.8%, 46.9% 44%, 48.3% 17.4%, 24.7% 53.9%, 0% 27.9%, 11.9% 74.2%, 24.9% 54.1%, 68.6% 100%, 74.8% 41.9%)',
          }}
        />
      </div>
      <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
        <p className="text-sm leading-6 text-white">
          <strong className="font-semibold">{title}</strong>
          <svg viewBox="0 0 2 2" className="mx-2 inline h-0.5 w-0.5 fill-current" aria-hidden="true">
            <circle cx={1} cy={1} r={1} />
          </svg>
          {description}
        </p>
        <a
          href={ctaLink}
          className="flex-none rounded-full bg-gray-900 px-3.5 py-1 text-sm font-semibold text-white shadow-sm hover:bg-gray-700 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900"
        >
          {cta} <span aria-hidden="true">&rarr;</span>
        </a>
        {changelogLink && (
          <a
            href={changelogLink}
            className="flex-none rounded-full bg-gray-300 px-3.5 py-1 text-sm font-semibold text-slate-800 shadow-sm hover:bg-slate-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-900"
          >
            {changelogTitle}
          </a>
        )}
      </div>
      <div className="flex flex-1 justify-end">
        <button type="button" onClick={() => setDismissed(true)} title="Close" className="-m-3 p-3 focus-visible:outline-offset-[-4px]">
          <span className="sr-only">Dismiss</span>
          <XMarkIcon className="h-5 w-5 text-white" aria-hidden="true" />
        </button>
      </div>
    </div>
  )
}
