import React from 'react';

export default function Video({
  url,
  title,
} : {
  url: string;
  title: string;
}){
  return (
    <div className="flex flex-col items-center">
      <iframe
        title={title}
        src={url}
        loading="lazy"
        className="border-none aspect-video w-full"
        allow="accelerometer; gyroscope; autoplay; encrypted-media; picture-in-picture;">
      </iframe>
      <p className="text-sm mt-2 mb-2 text-slate-400">{title}</p>
    </div>
  )
}
