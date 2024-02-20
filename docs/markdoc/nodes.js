import { nodes as defaultNodes } from '@markdoc/markdoc'
import { Fence } from '@/components/Fence'
import { Tag } from '@markdoc/markdoc';
import Image from '@/components/Image';
// import * as fs from 'node:fs/promises';

const nodes = {
  document: {
    render: undefined,
  },
  th: {
    ...defaultNodes.th,
    attributes: {
      ...defaultNodes.th.attributes,
      scope: {
        type: String,
        default: 'col',
      },
    },
  },
  fence: {
    render: Fence,
    attributes: {
      language: {
        type: String,
      },
    },
  },
  image: {
    render: Image,
    attributes: {
      src: { type: String },
      alt: { type: String },
      title: { type: String },
    },
    async transform(node, config) {
      const attributes = node.transformAttributes(config);
      const children = node.transformChildren(config);
      
      // return a next image on server side
      if (typeof window === 'undefined') {
        const sizeOf = require('image-size');
        const https = require('https');
  
        const src = decodeURIComponent(attributes.src);
        let imageInfo = null;
        if (src.startsWith('http')) {
          const resp = await (new Promise((resolve) => {
            https.get(attributes.src, res => {
              const chunks = [];
              res.on('data', chunk => chunks.push(chunk))
                .on('end', () => resolve(Buffer.concat(chunks)));
            })
          }));
          imageInfo = sizeOf(resp);
        } else {
          imageInfo = sizeOf('public' + src);
        }
        attributes.src = src;
  
        return new Tag(this.render, { ...attributes, ...imageInfo }, children);
      }

      const tags = [
        new Tag(
          `img`,
          { ...attributes, class: 'mb-2' },
          children
        )
      ];
      if (node.attributes.title) {
        tags.push(new Tag(
          'span',
          { class: 'text-sm text-center mt-4 mb-4 text-slate-400 block' },
          [node.attributes.title]
        ));
      }
  
      return tags;
    }
  }
}

export default nodes
