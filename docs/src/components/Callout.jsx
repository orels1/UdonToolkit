import clsx from 'clsx'

import { Icon } from '@/components/Icon'

const styles = {
  note: {
    container:
      'bg-zinc-50 dark:bg-zinc-800/60 dark:ring-1 dark:ring-zinc-300/10',
    title: 'text-zinc-900 dark:text-zinc-400',
    body: 'text-zinc-800 [--tw-prose-background:theme(colors.zinc.50)] prose-a:text-zinc-900 prose-code:text-zinc-900 dark:text-zinc-300 dark:prose-code:text-zinc-300 dark:prose-strong:text-zinc-100',
  },
  warning: {
    container:
      'bg-zinc-50 dark:bg-zinc-800/60 dark:ring-1 dark:ring-zinc-300/10',
    title: 'text-zinc-900 dark:text-zinc-500',
    body: 'text-zinc-800 [--tw-prose-underline:theme(colors.zinc.400)] [--tw-prose-background:theme(colors.zinc.50)] prose-a:text-zinc-900 prose-code:text-zinc-900 dark:text-zinc-300 dark:[--tw-prose-underline:theme(colors.sky.700)] dark:prose-code:text-zinc-300 dark:prose-strong:text-zinc-100',
  },
}

const icons = {
  note: (props) => <Icon icon="lightbulb" {...props} />,
  warning: (props) => <Icon icon="warning" color="zinc" {...props} />,
}

export function Callout({ type = 'note', title, children }) {
  let IconComponent = icons[type]

  return (
    <div className={clsx('my-8 flex rounded-3xl p-6', styles[type].container)}>
      <IconComponent className="h-8 w-8 flex-none" />
      <div className="ml-4 flex-auto">
        {title && (
          <p className={clsx('m-0 font-display text-xl', styles[type].title)}>
            {title}
          </p>
        )}
        <div className={clsx('prose', title && 'mt-2.5', styles[type].body)}>
          {children}
        </div>
      </div>
    </div>
  )
}
