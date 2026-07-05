import axios from 'axios'
import { Envelope } from './types/envelope';
import { EnvelopeError } from './types/errors';

export const apiClient = axios.create({
    baseURL: 'http://localhost:5057/api',
    headers: { 'Content-Type': 'application/json' },
    // ASP.NET биндит массивы как ?key=a&key=b (без скобок).
    // По умолчанию axios шлёт key[]=a — indexes:null убирает скобки.
    paramsSerializer: { indexes: null },
})

apiClient.interceptors.response.use(
    (response) => {
        const data = response.data as Envelope;

        if (data.errorList?.length) {
            throw new EnvelopeError(data.errorList);
        }

        return response;
    },
    (error) => {
        if (axios.isAxiosError(error) && error.response?.data){
            const envelope = error.response.data as Envelope;

            if (envelope.errorList?.length){
                throw new EnvelopeError(envelope.errorList)
            }
        }
        return Promise.reject(error)
    }
);