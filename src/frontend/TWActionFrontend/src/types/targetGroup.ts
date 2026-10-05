import type { TemplateWave } from "./targetTemplate";

export interface TargetGroup {
  id: string;
  scheduleId: string;
  name: string;
  villageCoordinates: string[];
  waves: TemplateWave[];
  baseTemplateId: string | null;
  baseTemplateName: string | null;
}

export interface CreateTargetGroupRequest {
  name: string;
  villageCoordinates: string[];
  waves: TemplateWave[];
  baseTemplateId: string | null;
  baseTemplateName: string | null;
}

export interface UpdateTargetGroupRequest {
  name: string;
  villageCoordinates: string[];
  waves: TemplateWave[];
  baseTemplateId: string | null;
  baseTemplateName: string | null;
}

/**
 * Parses a raw coordinate string into valid "X|Y" entries.
 * Accepts space-, newline-, comma- or semicolon-separated values.
 */
export function parseCoordinates(raw: string): string[] {
  return raw
    .split(/[\s,;]+/)
    .map((s) => s.trim())
    .filter((s) => /^\d+\|\d+$/.test(s));
}

/**
 * Returns raw tokens that are NOT valid coordinates.
 */
export function invalidCoordinates(raw: string): string[] {
  return raw
    .split(/[\s,;]+/)
    .map((s) => s.trim())
    .filter((s) => s.length > 0 && !/^\d+\|\d+$/.test(s));
}
