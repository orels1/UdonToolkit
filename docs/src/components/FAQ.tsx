import { Disclosure } from '@headlessui/react';
import { MinusSmallIcon, PlusSmallIcon } from '@heroicons/react/24/outline';
import Link from 'next/link';

const faqs = [
  {
    id: 1,
    question: "Are the shaders URP/HDRP compatible?",
    answer:
      "Unfortunately, no. There are plans to add URP support in the future! For now, you can use the shaders in the Built-In Render Pipeline only.",
  },
  {
    id: 2,
    question: "Are the shaders Android/Quest compatible?",
    answer:
      "Yes! The shaders are compatible with Android and Quest. However, you will need to use the Built-In Render Pipeline.",
  },
  {
    id: 3,
    question: "Can I use the shaders in commercial projects?",
    answer:
      "Yes! Most of the shader code is licensed under MIT, with Unity built-in sampler sources being licensed under Unity's companion license, which also permits commercial use.",
    links: [
      {
        text: 'Shader License',
        url: 'https://github.com/orels1/UdonToolkit/blob/main/Packages/sh.orels.shaders/LICENSE',
        target: '_blank',
      },
      {
        text: 'Unity Companion License',
        url: 'https://unity3d.com/legal/licenses/Unity_Companion_License',
        target: '_blank',
      }
    ]
  },
  {
    id: 4,
    question: "Do shaders work in Deferred Rendering?",
    answer:
      "Unfortunately, no. The shaders are built primarily for VR applications and as such - do not support deferred rendering.",
  },
  {
    id: 5,
    question: "How do I update?",
    answer:
      "Simply download the latest version of the package from Github or use the VCC to update to the latest version! During the major version updates I recommend checking out the Migration page for any extra info.",
    links: [
      {
        text: "Github",
        url: "https://github.com/orels1/UdonToolkit",
        target: '_blank',
      },
      {
        text: "Migration Guides",
        url: "/docs/migration",
      },
    ]
  },
  {
    id: 6,
    question: 'Which unity versions are supported?',
    answer: 'The package is built and tested with unity 2019.4. However it should work with any newer versions as long as the Built-In Rendering Pipeline is used.'
  },
  {
    id: 7,
    question: 'The cloud shader does not blend with the environment',
    answer: 'Make sure you have Dept enabled in the scene. Check the bottom of the Clouds documentation page to learn more.',
    links: [
      {
        text: 'Clouds Documentation',
        url: '/docs/vfx/clouds',
      }
    ],
  },
  {
    id: 8,
    question: 'The cloud shader does not move or splits the geometry',
    answer: 'The cloud shader is built to work on a high resolution mesh. Make sure you have enough polygons for the shader to work with.'
  }
]

export default function FAQ() {
  console.log(faqs);
  return (
    <div>
      <div className="mx-auto max-w-7xl">
        <div className="mx-auto max-w-4xl divide-y divide-white/10">
          <div className="mt-10 space-y-6 divide-y divide-white/10">
            {faqs.map((faq) => (
              <Disclosure as="div" key={faq.question} className="pt-6">
                {({ open }) => (
                  <>
                    <div>
                      <Disclosure.Button className="flex w-full items-start justify-between text-left dark:text-white">
                        <span className="text-base font-semibold leading-7">{faq.question}</span>
                        <span className="ml-6 flex h-7 items-center">
                          {open ? (
                            <MinusSmallIcon className="h-6 w-6" aria-hidden="true" />
                          ) : (
                            <PlusSmallIcon className="h-6 w-6" aria-hidden="true" />
                          )}
                        </span>
                      </Disclosure.Button>
                    </div>
                    <Disclosure.Panel as="div" className="mt-2 pr-12">
                      <p className="text-base leading-7 dark:text-gray-300">
                        {faq.answer}
                      </p>
                      {faq.links && (
                        <div className="flex gap-4">
                          <div>Useful Links:</div>
                          {faq.links.map((link) => (
                            <Link
                              href={link.url}
                              key={link.text}
                              {...(link.target && { target: link.target })}
                            >
                              {link.text}
                            </Link>
                          ))}
                        </div>
                      )}
                    </Disclosure.Panel>
                  </>
                )}
              </Disclosure>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
