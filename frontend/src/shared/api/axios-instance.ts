import axios from 'axios'
import { Envelope } from './types/envelope';
import { EnvelopeError } from './types/errors';

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost/api";

export const apiClient = axios.create({
    baseURL: BASE_URL,
    headers: { 'Content-Type': 'application/json' },
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