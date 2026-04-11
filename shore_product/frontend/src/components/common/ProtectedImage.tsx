import React, { useEffect, useState } from 'react';
import { fetchProtectedMediaObjectUrl, isProtectedMediaPath } from '../../services/protectedMedia';

interface ProtectedImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  fallbackSrc?: string;
}

const ProtectedImage: React.FC<ProtectedImageProps> = ({ src, fallbackSrc, ...props }) => {
  const [resolvedSrc, setResolvedSrc] = useState<string | undefined>(typeof src === 'string' ? src : undefined);

  useEffect(() => {
    if (!src || typeof src !== 'string' || !isProtectedMediaPath(src)) {
      setResolvedSrc(typeof src === 'string' ? src : fallbackSrc);
      return;
    }

    const controller = new AbortController();
    let objectUrl: string | null = null;

    fetchProtectedMediaObjectUrl(src, controller.signal)
      .then((url) => {
        objectUrl = url;
        setResolvedSrc(url);
      })
      .catch(() => {
        setResolvedSrc(fallbackSrc);
      });

    return () => {
      controller.abort();
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
      }
    };
  }, [fallbackSrc, src]);

  return <img {...props} src={resolvedSrc || fallbackSrc} />;
};

export default ProtectedImage;