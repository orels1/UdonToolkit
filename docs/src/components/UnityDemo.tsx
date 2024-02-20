import React from 'react';
import Unity, { UnityContent } from "react-unity-webgl";
import clsx from 'clsx';

export default function UnityDemo({
  url,
  width = 672,
  height = 700,
  className,
}: {
  url: string;
  width: number;
  height: number;
  className?: string;
}) {
  const [unityContent, setUnityContent] = React.useState<UnityContent | null>(null);
  const [demoShown, setDemoShown] = React.useState(false);

  React.useEffect(() => {
    setUnityContent(new UnityContent(
      `${url}/Build.json`,
      `${url}/UnityLoader.js`
    ));
  }, [url]);
  
  return (
    <div className="flex flex-col">
      <button
        type="button"
        title="Toggle Demo"
        onClick={() => setDemoShown(!demoShown)}
        className="w-full bg-slate-800 rounded-md py-2 ring-1 ring-slate-500 hover:ring-orange-400 transition-colors"
      >{demoShown ? 'Hide Demo' : 'Show Demo'}</button>
      {demoShown && (
        <div className={clsx(className,'w-full mt-2 rounded-md overflow-hidden')} style={{ aspectRatio: `${width}/${height}` }}>
          {unityContent && (
            <Unity width={width} height={height} unityContent={unityContent} />
          )}
        </div>
      )}
    </div>
  )
}