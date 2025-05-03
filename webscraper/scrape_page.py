import requests
from bs4 import BeautifulSoup

#
# Scrapes the text of the given page.
# Returns the text.
#
def scrape_page_to_text(URL):
    try:
        response = requests.get(URL)
        response.raise_for_status()  # Raise exception for HTTP errors
        soup = BeautifulSoup(response.text, "html.parser")

        # Try to locate the main content area
        content = soup.find("main")
        if not content:
            content = soup.body  # Fallback to <body> if <main> is not available

        if not content:
            raise ValueError("No suitable content container (<main> or <body>) found on the page.")

        # Extract and return text with newlines between elements
        return content.get_text(separator="\n", strip=True)

    except Exception as e:
        print(f"⚠️ Error scraping {URL}: {e}")
        return ""
