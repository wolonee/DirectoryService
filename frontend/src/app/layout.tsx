import type { Metadata } from "next";
import "./globals.css";
import Layout from "@/features/layout/app-layout";

export const metadata: Metadata = {
  title: {
    default: "Directory Service",
    template: "%s | Directory Service",
  },
  description: "Корпоративный справочник организационной структуры",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="dark h-full antialiased">
      <body className="min-h-full flex flex-col">
        <Layout>{children}</Layout>
      </body>
    </html>
  );
}
