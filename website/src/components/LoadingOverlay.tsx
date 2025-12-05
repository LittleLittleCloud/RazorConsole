import { Terminal } from 'lucide-react';
import { useState, useEffect, useRef } from 'react';

interface Props {
    text: string;
}

const LoadingOverlay: React.FC<Props> = ( { text }) => {
    const [dots, setDots] = useState('');
    const increasingRef = useRef(true);

    useEffect(() => {
        const interval = setInterval(() => {
            setDots(currentDots => {
                if (increasingRef.current) {
                    if (currentDots.length < 3) {
                        return currentDots + '.';
                    } else {
                        increasingRef.current = false;
                        return '..';
                    }
                } else {
                    if (currentDots.length > 1) {
                         return currentDots.slice(0, -1);
                    }
                    else if (currentDots.length == 1){
                        return ''
                    }
                    else {
                        increasingRef.current = true;
                        return '..';
                    }
                }
            });
        }, 350);

        return () => clearInterval(interval);
    }, []);

    return (
        <div className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-slate-50 dark:bg-slate-950 select-none">
            <div className="absolute flex items-center justify-center pointer-events-none">
                 <div className="w-40 h-40 bg-gradient-to-r from-blue-500/20 to-violet-500/20 dark:from-blue-600/40 dark:to-violet-600/40 rounded-full blur-3xl " />
            </div>

            <div className="relative z-10 flex flex-col items-center">
                <div className="flex items-end justify-center pb-4">
                    <Terminal className="w-20 h-20 text-blue-600 dark:text-blue-400 mr-2" strokeWidth={3} />

                    <span className="text-6xl font-bold bg-gradient-to-r from-blue-600 to-violet-600 dark:from-blue-400 dark:to-violet-400 bg-clip-text text-transparent leading-none w-[3ch] text-left font-mono mb-[2px]">
                        {dots}
                    </span>
                </div>

                <p className="text-lg font-medium bg-gradient-to-r from-blue-600 to-violet-600 dark:from-blue-400 dark:to-violet-400 bg-clip-text text-transparent animate-pulse">
                    {text}
                </p>
            </div>
        </div>
    );
}

export default LoadingOverlay;
