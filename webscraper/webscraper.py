import os
import requests
from bs4 import BeautifulSoup
from urllib.parse import urljoin, urlparse

BASE_URL = "https://wiki.bambulab.com/en/a1"
DOMAIN = "https://wiki.bambulab.com"
OUTPUT_DIR = "replacement_guides"

os.makedirs(OUTPUT_DIR, exist_ok=True)

def save_response_html(response_text, filename="debug_response.html"):
    """Sla de HTML-response op in een bestand voor debugging."""
    debug_file = os.path.join(OUTPUT_DIR, filename)
    with open(debug_file, "w", encoding="utf-8") as f:
        f.write(response_text)
    print(f"✔️ HTML-response opgeslagen voor debugging: {debug_file}")

def get_replacement_links():
    """Scrape de Part Replacement Guides links van de hoofd A1 pagina"""
    response = requests.get(BASE_URL)
    
    # Sla de response op voor debugging
    save_response_html(response.text)

    soup = BeautifulSoup(response.text, "html.parser")

    # Zoek specifiek naar een <h2> met de tekst "Part Replacement Guides"
    heading = soup.find("h2", string=lambda t: t and "Part Replacement Guides" in t)

    if not heading:
        raise ValueError("Part Replacement Guides section not found.")

    links = []
    for sibling in heading.find_next_siblings():
        # Stop bij een andere hoofdheading (zoals Troubleshooting)
        if sibling.name and sibling.name.startswith("h"):
            break
        # Zoek alle <a href="..."> in de content
        for a in sibling.find_all("a", href=True):
            full_url = urljoin(DOMAIN, a['href'])
            links.append((a.text.strip(), full_url))

    return links

def save_page_text(title, url):
    """Download de inhoud van de pagina en sla op als .txt bestand"""
    try:
        response = requests.get(url)
        soup = BeautifulSoup(response.text, "html.parser")

        # Zoek alleen de hoofdinhoud van de pagina (meestal in <main> of <article>)
        main_content = soup.find("main")
        if not main_content:
            main_content = soup.body

        text = main_content.get_text(separator="\n", strip=True)

        # Maak een geldige bestandsnaam
        safe_title = "".join(c if c.isalnum() else "_" for c in title)
        filename = os.path.join(OUTPUT_DIR, f"{safe_title}.txt")

        with open(filename, "w", encoding="utf-8") as f:
            f.write(f"Title: {title}\nURL: {url}\n\n{text}")

        print(f"✔️ Opgeslagen: {filename}")
    except Exception as e:
        print(f"⚠️ Fout bij {url}: {e}")

if __name__ == "__main__":
    links = get_replacement_links()
    print(f"Gevonden {len(links)} replacement guide-links.")

    for title, url in links:
        save_page_text(title, url)
