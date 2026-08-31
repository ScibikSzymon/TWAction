import { useState, useCallback, type ReactNode } from "react";
import { pl } from "./pl";
import { en } from "./en";
import { I18nContext } from "./context";
import type { Translations } from "./types";

export type Language = "pl" | "en";

const translations: Record<Language, Translations> = { pl, en };

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
