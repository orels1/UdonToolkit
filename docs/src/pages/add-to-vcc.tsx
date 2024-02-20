import addToVccImage from '@/images/add-to-vcc-popup.png';
import Image from 'next/image';
import Head from 'next/head';
import { useEffect } from 'react';

const SHADERS_VCC_LINK = 'vcc://vpm/addRepo?url=https://orels1.github.io/UdonToolkit/index.json';

export default function AddToVCC() {
  useEffect(() => {
    window.location.assign(SHADERS_VCC_LINK);
  }, []);
  
  return (
    <div>
      <Head>
        <title>Add ORL Shaders to the VCC</title>
      </Head>
      <h2 className="text-xl">A prompt to open the VCC should appear shortly</h2>
      <p>It should look something like this:</p>
      <Image src={addToVccImage} alt="Add to VCC prompt" placeholder="blur" quality="95" />
      <p>Click &quot;Open&quot; to start the installation process</p>
      <p>Click <a href={SHADERS_VCC_LINK}>this link</a> if it does not appear</p>
      <p>If all else failed - <a href="/docs/installation">try the Manual Installation described here</a></p>
    </div>
  );
}