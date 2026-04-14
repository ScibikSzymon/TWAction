import { createContext, useContext, useState, useCallback, type ReactNode } from "react";
import { pl } from "./pl";
import { en } from "./en";

export type Language = "pl" | "en";
export type Translations = typeof pl;

const translations: Record<Language, Translations> = { pl, en };

interface I18nContextValue {
  language: Language;
  t: Translations;
  setLanguage: (lang: Language) => void;
}

const I18nContext = createContext<I18nContextValue | null>(null);

const getInitialLanguage = (): Language => {
  const stored = localStorage.getItem("language");
  if (stored === "pl" || stored === "en") return stored;
  return "pl";
};

export const I18nProvider = ({ children }: { children: ReactNode }) => {
  const [language, setLanguage] = useState<Language>(getInitialLanguage);

  const changeLanguage = useCallback((lang: Language) => {
    localStorage.setItem("language", lang);
    setLanguage(lang);
  }, []);

  return (
    <I18nContext.Provider value={{ language, t: translations[language], setLanguage: changeLanguage }}>
      {children}
    </I18nContext.Provider>
  );
};

export const useI18n = () => {
  const context = useContext(I18nContext);
  if (!context) {
    throw new Error("useI18n must be used within an I18nProvider");
  }
  return context;
};
