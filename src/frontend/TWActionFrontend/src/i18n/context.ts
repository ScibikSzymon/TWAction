import { createContext } from "react";
import type { Language } from "./I18nProvider";
import type { Translations } from "./types";

export interface I18nContextValue {
  language: Language;
  t: Translations;
  setLanguage: (lang: Language) => void;
}

export const I18nContext = createContext<I18nContextValue | null>(null);
