'use client'

import { useParamsStore } from "@/hooks/useParamsStore"
import { usePathname, useRouter } from "next/navigation";
import { AiOutlineCar } from "react-icons/ai"

const Logo = () => {
    const reset = useParamsStore(state => state.reset);
    const router = useRouter();
    const pathname = usePathname();

    const doReset = () => {
        if (pathname !== '/') router.push('/');
        reset();
    };

    return (
        <div onClick={doReset} className='flex items-center gap-2 text-3xl font-semibold text-red-500 cursor-pointer'>
            <AiOutlineCar size={34} />
            <div>Carsties Auctions</div>
        </div>
    )
}

export default Logo