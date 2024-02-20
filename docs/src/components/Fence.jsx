import { Fragment } from 'react';
import { Prism, Highlight, themes } from 'prism-react-renderer';
import clsx from 'clsx';
import CopyButton from '@/components/CopyButton';

(typeof global !== "undefined" ? global : window).Prism = Prism;

export function Fence({ children, language }) {
  // @ts-ignore
  (typeof global !== "undefined" ? global : window).Prism = Prism;
  require('prismjs/components/prism-csharp');
  require('prismjs/components/prism-hlsl');
  return (
    <Highlight
      code={children.trimEnd()}
      language={language ?? 'text'}
      theme={themes.oneDark}
    >
      {({ className, style, tokens, getTokenProps }) => (
        <div className="relative group">
          <CopyButton
            className="group-hover:opacity-100 opacity-0 absolute top-2 right-2 z-30"
            content={children.toString().trimEnd()}
          />
          <pre className={clsx(className, 'relative z-20')} style={style}>
            <code>
              {tokens.map((line, lineIndex) => (
                <Fragment key={lineIndex}>
                  {line
                    .filter((token) => !token.empty)
                    .map((token, tokenIndex) => (
                      <span key={tokenIndex} {...getTokenProps({ token })} />
                    ))}
                  {'\n'}
                </Fragment>
              ))}
            </code>
          </pre>
          <div className="absolute -inset-[1px] rounded-[12px] opacity-30 z-10 bg-gradient-to-br from-zinc-300 to-zinc-500/50" />
        </div>
      )}
    </Highlight>
  )
}
