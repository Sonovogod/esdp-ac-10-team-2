import axios from 'axios';
import authHeader from './authHeader';
import authService from "@/api/AuthService";

const instance = axios.create({
    baseURL: 'https://localhost:7015/',
});
axios.defaults.headers.common = authHeader();
axios.interceptors.response.use(response => {
    return response;
}, async error => {
    if (error.response.status === 401) {
        const user = JSON.parse(localStorage.getItem('refreshToken'));
        const tokenApiModel = {accessToken: user.accessToken, refreshToken: user.refreshToken};
        try {
            const response = await authService.refreshingToken(tokenApiModel);
            localStorage.getItem('userToken')
            localStorage.getItem('refreshToken')
        } catch (error) {
            console.error('Ошибка обновления токена:', error);
            return {};
        }
        return error;
    }});

export default instance;