import os
from urllib.parse import urlparse
from scrape_page import scrape_page_to_text
from globals import OUTPUT_DIR, DOMAIN, BASE_URL

def get_last_path_segment(url):
    path = urlparse(url).path
    return path.rstrip("/").split("/")[-1]

def sanitize_filename(name):
    return "".join(c if c.isalnum() or c in "-_" else "_" for c in name)

def process_links(links):
    current_folder = None

    for _, url in links:
        if "#" in url:
            # Folder marker
            current_folder = url.split("#")[-1]
            folder_path = os.path.join(OUTPUT_DIR, current_folder)
            os.makedirs(folder_path, exist_ok=True)
            print(f"📁 Created folder: {folder_path}")
        elif url.endswith("/"):
            # Skip root pages with no file content
            continue
        else:
            if not current_folder:
                print(f"⚠️ Skipping {url} (no folder context)")
                continue

            file_name = sanitize_filename(get_last_path_segment(url)) + ".txt"
            file_path = os.path.join(OUTPUT_DIR, current_folder, file_name)
            text = scrape_page_to_text(url)

            if text:
                with open(file_path, "w", encoding="utf-8") as f:
                    f.write(f"{url}\n\n{text}")
                print(f"📝 Saved: {file_path}")
            else:
                print(f"⚠️ Empty content for {url}")