'use client'

import { useParamsStore } from "@/hooks/useParamsStore"
import { FaSearch } from "react-icons/fa"

const Search = () => {
    const setParams = useParamsStore(state => state.setParams);
    const setSearchValue = useParamsStore(state => state.setSearchValue);
    const searchValue = useParamsStore(state => state.searchValue);

    const onChange = (event: any) => {
        setSearchValue(event.target.value);
    };

    const search = () => {
        setParams({ searchTerm: searchValue });
    };


    return (
        <div className='flex w-[50%] items-center border-2 rounded-full py-2 shadow-sm'>
            <input
                type="text"
                value={searchValue}
                placeholder="Search for cars by make, model or color"
                onKeyDown={(e: any) => {
                    if (e.key === 'Enter') search();
                }}
                onChange={onChange}
                className='flex-grow pl-5 bg-transparent focus:outline-none border-transparent focus:border-transparent focus:ring-0 text-sm text-gray-600' />
            <button onClick={search}>
                <FaSearch size={34} className='bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2' />
            </button>
        </div>
    )
}

export default Search