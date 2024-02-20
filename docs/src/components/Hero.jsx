import { Fragment, useState } from 'react'
import Image from 'next/image'
import clsx from 'clsx'

import { Button } from '@/components/Button'
// import heroStandardImage from '@/images/hero-standard.png'

const tabs = [
  { name: 'Trailer', content: (
    <div className="flex align-center justify-center relative w-full h-full pointer-events-none">
      <iframe
        src="https://iframe.mediadelivery.net/embed/165/e375494d-d876-4882-b1e0-ba1a81ab41e1?autoplay=true&loop=true&muted=true"
        loading="lazy"
        allow="accelerometer; gyroscope; autoplay; encrypted-media;"      
        className="aspect-video absolute inset-0 w-full h-full pointer-events-none"
      ></iframe>
    </div>
  )},
  // { name: 'Standard', content: (
  //   <Image
  //     src={heroStandardImage}
  //     alt="Standard"
  //     className="inset-0 absolute aspect-[640/350]"
  //     quality={100}
  //     width={640}
  //     height={350}
  //   />
  // )},
]

function TrafficLightsIcon(props) {
  return (
    <svg aria-hidden="true" viewBox="0 0 42 10" fill="none" {...props}>
      <circle cx="5" cy="5" r="4.5" />
      <circle cx="21" cy="5" r="4.5" />
      <circle cx="37" cy="5" r="4.5" />
    </svg>
  )
}

export function Hero() {
  const [selectedTab, setSelectedTab] = useState(0)
  return (
    <div className="overflow-hidden bg-zinc-900 dark:-mb-32 dark:mt-[-4.5rem] dark:pb-32 dark:pt-[4.5rem] dark:lg:mt-[-4.75rem] dark:lg:pt-[4.75rem]">
      <div className="py-16 sm:px-2 lg:relative lg:py-20 lg:px-0">
        <div className="mx-auto grid max-w-2xl grid-cols-1 items-center gap-y-16 gap-x-8 px-4 lg:max-w-8xl lg:grid-cols-2 lg:px-8 xl:gap-x-16 xl:px-12">
          <div className="relative z-10 md:text-center lg:text-left">
            <div className="relative">
              <p className="inline bg-gradient-to-r from-slate-200 via-zinc-500 to-slate-200 bg-clip-text font-display text-5xl tracking-tight text-transparent">
                UdonToolkit
              </p>
              <p className="mt-3 text-2xl tracking-tight text-slate-400">
                Purpose-built Udon Behaviours and tools to make your own
              </p>
              <div className="mt-8 flex gap-4 md:justify-center lg:justify-start">
                <Button href="#quick-start">Get started</Button>
                <Button href="https://github.com/orels1/UdonToolkit" target="_blank" variant="secondary">
                  View on GitHub
                </Button>
              </div>
            </div>
          </div>
          <div className="relative lg:static xl:pl-10">
            <div className="relative">
              <div className="absolute inset-0 rounded-2xl bg-gradient-to-tr from-zinc-300 via-zinc-300/70 to-slate-300 opacity-10 blur-lg" />
              <div className="absolute inset-0 rounded-2xl bg-gradient-to-tr from-zinc-300 via-zinc-300/70 to-slate-300 opacity-10" />
              <div className="relative rounded-2xl overflow-hidden bg-[#0A101F]/80 ring-1 ring-white/10 backdrop-blur aspect-[640/350]">
                <div className="absolute -top-px left-20 right-11 h-px bg-gradient-to-r from-zinc-300/0 via-zinc-300/70 to-zinc-300/0" />
                <div className="absolute -bottom-px left-11 right-20 h-px bg-gradient-to-r from-slate-400/0 via-slate-400 to-slate-400/0" />
                <div className="pl-4 pt-4 pb-8 relative z-20 bg-gradient-to-b from-zinc-900 to-zinc-900/0">
                  <div className="mt-1 flex text-xs flex-wrap">
                    {tabs.map((tab, index) => (
                      <div
                        key={tab.name}
                        onClick={() => setSelectedTab(index)}
                        className={clsx(
                          'flex h-6 rounded-full cursor-pointer ml-2',
                          tabs[selectedTab].name === tab.name
                            ? 'bg-gradient-to-r from-zinc-400/30 via-zinc-400 to-zinc-400/30 p-px font-medium text-zinc-300'
                            : 'text-slate-300 hover:text-zinc-300'
                        )}
                      >
                        <div
                          className={clsx(
                            'flex items-center rounded-full px-2.5',
                            tabs[selectedTab].name === tab.name && 'bg-zinc-800'
                          )}
                        >
                          {tab.name}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
                <div className="absolute inset-0 z-10">
                  {tabs[selectedTab].content}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
