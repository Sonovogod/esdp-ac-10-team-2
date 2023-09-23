import axios from "@/utils/axios";
import storeExtension from "@/utils/storeExtension";

const projectService = {
    async getProjects() {
        try {
            return await axios.get(`/api/projects`);
        }
        catch (error){
            console.log(error)
        }
    },

    async createProject(project) {
        try {
            return await axios.post('/api/projects', project);
        }
        catch (error){
            console.log(error)
        }

    },

    async getProject(oid) {
        try {
            return await axios.get(`/api/projects/${oid}`);
        }
        catch (error){
            console.log(error)
        }
    },

    async updateProject(project) {
        try {
            return await axios.put('/api/projects', project)
        }
        catch (error){
            console.log(error)
        }
    },

    async deleteProject(oid) {
        try {
            return await axios.delete(`/api/projects/${oid}`)
        }
        catch (error){
            console.log(error)
        }

    },

    async getProjectsForGrid(loadOptions) {
        try {
            let options = storeExtension.getParams(loadOptions);
            const response = await axios.get(`/api/projects/Index/${options}`);
            console.log(response)
            return response.data;
        } catch (error) {
            console.log(error)
        }
    },

    async getProjectFile(id) {
        try {
            const response = await axios.get(`/api/export-project/${id}`, {responseType: "blob"});
            return response;
        } catch (error) {
            console.log(error)
        }
    }
};
export default projectService;