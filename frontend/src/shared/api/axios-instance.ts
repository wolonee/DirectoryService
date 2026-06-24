import axios from 'axios'
import { Envelope } from './types/envelope';
import { EnvelopeError } from './types/errors';

export const apiClient = axios.create({
    baseURL: 'http://localhost:5057/api',
    headers: { 'Content-Type': 'application/json' },
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