import requests
from bs4 import BeautifulSoup
from bs4.element import Comment, NavigableString, Tag
import os
from urllib.parse import urljoin, urlparse
from globals import BASE_URL

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

def text_and_images_from_html(soup, base_url, image_folder="downloaded_images"):
    os.makedirs(image_folder, exist_ok=True)
    output_lines = []

    def process_element(el):
        if isinstance(el, NavigableString):
            if tag_visible(el):
                text = el.strip()
                if text:
                    output_lines.append(text)
        elif isinstance(el, Tag):
            if el.name == "img" and el.has_attr("src"):
                img_url = urljoin(base_url, el["src"])
                img_name = os.path.basename(urlparse(img_url).path)

                # Download the image
                try:
                    img_data = requests.get(img_url).content
                    with open(os.path.join(image_folder, img_name), "wb") as f:
                        f.write(img_data)
                    output_lines.append(f"[Image: {img_name}]")
                except Exception as e:
                    print(f"⚠️ Failed to download image {img_url}: {e}")
            else:
                # Recursively process children
                for child in el.contents:
                    process_element(child)

    body = soup.body
    if body:
        process_element(body)
    else:
        print("⚠️ No <body> found in HTML.")

    return "\n".join(output_lines)


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
        return text_and_images_from_html(soup, BASE_URL)

    except Exception as e:
        print(f"⚠️ Error scraping {URL}: {e}")
        return ""
