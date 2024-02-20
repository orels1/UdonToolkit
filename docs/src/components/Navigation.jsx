import Link from 'next/link'
import { useRouter } from 'next/router'
import clsx from 'clsx'

export function Navigation({ navigation, className }) {
  let router = useRouter()

  return (
    <nav className={clsx('text-base lg:text-sm', className)}>
      <ul role="list">
        {navigation.map((section) => {
          if ((section.links?.length ?? 0) === 0 && section.link) {
            return (
              <li key={section.title}>
                <h1 className="font-display text-xl font-medium text-zinc-900 dark:text-white">
                  <Link href={section.link.href} className="hover:text-orange-400">
                    {section.title}
                  </Link>
                </h1>
              </li>
            )
          }
          return (
            <li key={section.title}>
              <h2 className="font-display font-medium text-zinc-900 dark:text-white">
                {section.title}
              </h2>
              <ul
                role="list"
                className="mt-2 mb-9 space-y-2 border-l-2 border-zinc-100 dark:border-zinc-800 lg:mt-4 lg:space-y-4 lg:border-zinc-200"
              >
                {section.links.map((link) => (
                  <li key={link.href} className="relative">
                    <Link
                      href={link.href}
                      className={clsx(
                        'block w-full pl-3.5 before:pointer-events-none before:absolute before:-left-1 before:top-1/2 before:h-1.5 before:w-1.5 before:-translate-y-1/2 before:rounded-full',
                        link.href === router.pathname
                          ? 'font-semibold text-zinc-100 before:bg-zinc-100'
                          : 'text-zinc-500 before:hidden before:bg-zinc-300 hover:text-zinc-600 hover:before:block dark:text-zinc-400 dark:before:bg-zinc-700 dark:hover:text-zinc-300'
                      )}
                    >
                      {link.title}
                    </Link>
                  </li>
                ))}
              </ul>
            </li>
          )
        })}
      </ul>
    </nav>
  )
}
