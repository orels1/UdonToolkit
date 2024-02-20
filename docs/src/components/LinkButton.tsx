import Link from 'next/link'
import clsx from 'clsx'

const styles = {
  primary:
    'rounded-full bg-orange-300 py-2 px-4 text-sm font-semibold text-slate-900 hover:bg-orange-200 focus:outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-300/50 active:bg-orange-500',
  secondary:
    'rounded-full bg-slate-800 py-2 px-4 text-sm font-medium text-white hover:bg-slate-700 focus:outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white/50 active:text-slate-400',
}

export function LinkButton({
  variant = 'primary',
  className,
  href,
  text
}: {
  variant: keyof typeof styles;
  className?: string;
  href: string;
  text: string;
}) {

  return (
    <Link href={href} className={clsx(styles[variant], className, 'box-shadow-none')}>{text}</Link>
  )
}
