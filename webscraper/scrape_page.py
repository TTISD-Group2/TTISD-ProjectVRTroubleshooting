import requests
from bs4 import BeautifulSoup
from bs4.element import Comment

#
# Source: [https://stackoverflow.com/questions/1936466/how-to-scrape-only-visible-webpage-text-with-beautifulsoup]
#
def tag_visible(element):
    if element.parent.name in ['style', 'script', 'head', 'title', 'meta', '[document]']:
        return False
    if isinstance(element, Comment):
        return False
    return True

def text_from_html(soup):
    texts = soup.findAll(text=True)
    visible_texts = filter(tag_visible, texts)  
    return u"\n".join(t.strip() for t in visible_texts)

#
# Scrapes the text of the given page.
# Returns the text.
#
def scrape_page_to_text(URL):
    try:
        response = requests.get(URL)
        response.raise_for_status()  # Raise exception for HTTP errors
        soup = BeautifulSoup(response.content, "html.parser")

        # Try to locate the main content area
        body = soup.body
        if not body:
            raise ValueError("No suitable content container (<main> or <body>) found on the page.")

        # Extract and return text with newlines between elements
        return text_from_html(soup)

    except Exception as e:
        print(f"⚠️ Error scraping {URL}: {e}")
        return ""
