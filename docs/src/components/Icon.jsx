import { useId } from 'react'
import clsx from 'clsx'

import { InstallationIcon } from '@/components/icons/InstallationIcon'
import { LightbulbIcon } from '@/components/icons/LightbulbIcon'
import { PluginsIcon } from '@/components/icons/PluginsIcon'
import { PresetsIcon } from '@/components/icons/PresetsIcon'
import { ThemingIcon } from '@/components/icons/ThemingIcon'
import { WarningIcon } from '@/components/icons/WarningIcon'
import { PBRIcon } from '@/components/icons/PBRIcon'
import { ToonIcon } from '@/components/icons/ToonIcon'
import { VFXIcon } from '@/components/icons/VFXIcon'
import { UIIcon } from '@/components/icons/UIIcon'
import { CogIcon } from '@/components/icons/CogIcon'
import { PrefabIcon } from '@/components/icons/PrefabIcon'
import { AttributesIcon } from '@/components/icons/AttributesIcon'

const icons = {
  installation: InstallationIcon,
  presets: PresetsIcon,
  plugins: PluginsIcon,
  theming: ThemingIcon,
  lightbulb: LightbulbIcon,
  warning: WarningIcon,
  pbr: PBRIcon,
  toon: ToonIcon,
  vfx: VFXIcon,
  ui: UIIcon,
  cog: CogIcon,
  prefab: PrefabIcon,
  attributes: AttributesIcon,
}

const iconStyles = {
  blue: '[--icon-foreground:theme(colors.slate.900)] [--icon-background:theme(colors.white)]',
  amber:
    '[--icon-foreground:theme(colors.amber.900)] [--icon-background:theme(colors.amber.100)]',
  zinc:
    '[--icon-foreground:theme(colors.zinc.900)] [--icon-background:theme(colors.zinc.100)]',
}

export function Icon({ color = 'zinc', icon, size = '32', className, ...props }) {
  let id = useId()
  let IconComponent = icons[icon]

  return (
    <svg
      aria-hidden="true"
      viewBox={`0 0 ${size} ${size}`}
      fill="none"
      className={clsx(className, iconStyles[color])}
      {...props}
    >
      <IconComponent id={id} color={color} />
    </svg>
  )
}

const gradients = {
  blue: [
    { stopColor: '#0EA5E9' },
    { stopColor: '#22D3EE', offset: '.527' },
    { stopColor: '#818CF8', offset: 1 },
  ],
  amber: [
    { stopColor: '#FDE68A', offset: '.08' },
    { stopColor: '#F59E0B', offset: '.837' },
  ],
  zinc: [
      { stopColor: '#a1a1aa', offset: '.08' },
      { stopColor: '#71717a', offset: '.837' },
  ],
}

export function Gradient({ color = 'blue', ...props }) {
  return (
    <radialGradient
      cx={0}
      cy={0}
      r={1}
      gradientUnits="userSpaceOnUse"
      {...props}
    >
      {gradients[color].map((stop, stopIndex) => (
        <stop key={stopIndex} {...stop} />
      ))}
    </radialGradient>
  )
}

export function LightMode({ className, ...props }) {
  return <g className={clsx('dark:hidden', className)} {...props} />
}

export function DarkMode({ className, ...props }) {
  return <g className={clsx('hidden dark:inline', className)} {...props} />
}
