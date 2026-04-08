import { apiClient } from "../config/api";
import type {
  TargetTemplate,
  CreateTargetTemplateRequest,
  UpdateTargetTemplateRequest,
} from "../types/targetTemplate";

export const targetTemplateService = {
  async getTemplates(): Promise<TargetTemplate[]> {
    const response = await apiClient.get<TargetTemplate[]>("/target-templates");
    return response.data;
  },

  async getTemplateById(id: string): Promise<TargetTemplate> {
    const response = await apiClient.get<TargetTemplate>(`/target-templates/${id}`);
    return response.data;
  },

  async createTemplate(request: CreateTargetTemplateRequest): Promise<TargetTemplate> {
    const response = await apiClient.post<TargetTemplate>("/target-templates", request);
    return response.data;
  },

  async updateTemplate(id: string, request: UpdateTargetTemplateRequest): Promise<TargetTemplate> {
    const response = await apiClient.put<TargetTemplate>(`/target-templates/${id}`, request);
    return response.data;
  },

  async deleteTemplate(id: string): Promise<void> {
    await apiClient.delete(`/target-templates/${id}`);
  },
};
