import axios from 'axios';
import tokenService from '@/api/tokenService';
import authorizationService from "@/api/AuthorizationService";

const instance = axios.create({
    baseURL: 'http://localhost:80/',
    //baseURL: 'https://localhost:7015/',
});

instance.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalConfig = error.config;

        if (originalConfig.url !== "/api/Auth/login" && error.response) {

            if (error.response.status === 401 && !originalConfig._retry) {
                originalConfig._retry = true
                try {
                    const refreshToken = await tokenService.getRefreshToken();
                    const userToken = await tokenService.getAccessToken();
                    const tokenApiModel = {accessToken: userToken, refreshToken: refreshToken};
                    const response = await authorizationService.refreshingToken(tokenApiModel);
                    await tokenService.updateTokens(response);
                    
                    return instance(originalConfig);
                } catch (error) {
                    console.error('Ошибка обновления токена:', error);
                    
                    return Promise.reject(error);
                }
            }
            return Promise.reject(error);
        }
    }
);

instance.interceptors.request.use(
    async (config) => {
        const token = await tokenService.getAccessToken();
        if (token) {
            config.headers.Authorization = 'Bearer ' + token;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);
export default instance;