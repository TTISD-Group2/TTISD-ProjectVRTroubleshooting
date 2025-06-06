import os
import requests
from bs4 import BeautifulSoup
from urllib.parse import urljoin
from process_links import process_links
from globals import OUTPUT_DIR, DOMAIN, BASE_URL


#
# Saves a response to file.
# The filename needs to account for the filetype extension
#
def save_response(response_text, filename):
    debug_file = os.path.join(OUTPUT_DIR, filename)
    with open(debug_file, "w", encoding="utf-8") as f:
        f.write(response_text)
    return debug_file

#
# returns al the outgoing links of a given section.
# Each entry in the return array is of the form (text, URL)
#
def get_links_from_section(page_content, header_id = "part-replacement-guides"):
    soup = BeautifulSoup(page_content, "html.parser")

    # Zoek de specifieke <h2> tag
    start_tag = soup.find("h2", id=header_id)
    if not start_tag:
        raise ValueError("De sectie 'Part Replacement Guides' is niet gevonden.")

    # Verzamel alle links vanaf de <h2> tot het einde van de overkoepelende <div>
    links = []
    for sibling in start_tag.find_next_siblings():
        # Stop als je een nieuwe <h2> of een ander belangrijk element tegenkomt
        if sibling.name == "h2":
            break
        # Zoek naar alle <a> tags binnen de huidige sibling
        for a_tag in sibling.find_all("a", href=True):
            full_url = urljoin(DOMAIN, a_tag["href"])
            links.append((a_tag.text.strip(), full_url))

    return links

#
# Main function
#
def main():
    # Create the output directory if it does not exist yet.
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    # Get the main page
    page = requests.get(BASE_URL)

    # Save the page to a html file for debugging purposes
    page_contents_location = save_response(page.content.decode("utf-8"), "debug_file_contents.html")
    print(f"✔️ HTML-response saved for debugging purposes: {page_contents_location}")

    # Get the outgoing links from the recuested section
    links = get_links_from_section(page.content, header_id="part-replacement-guides")
    if len(links) != 0 :
        print(f"✔️ Outgoing seciton links found - {len(links)} number of links found")

    process_links(links)

    

if __name__ == "__main__":
    main()
