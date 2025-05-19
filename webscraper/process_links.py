import os
import requests
from urllib.parse import urlparse
from scrape_page import scrape_page_to_text
from globals import OUTPUT_DIR, DOMAIN, BASE_URL
from openai import OpenAI

PAGE_DELIMITER = "===PAGE_BREAK==="
client = OpenAI(api_key="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") 

def get_last_path_segment(url):
    path = urlparse(url).path
    return path.rstrip("/").split("/")[-1]

def sanitize_filename(name):
    return "".join(c if c.isalnum() or c in "-_" else "_" for c in name)

def process_with_gpt(text, model="gpt-4-turbo", max_tokens=4000):
    response = client.chat.completions.create(
        model=model,
        messages=[
            {
                "role": "system",
                "content": (
                    f"You are a helpful assistant that provides detailed step-by-step guides. "
                    f"Split your response into logical pages using the delimiter '{PAGE_DELIMITER}'. "
                    f"Each page should contain a complete section or concept."
                )
            },
            {
                "role": "user",
                "content": (
                    f"Please convert the following text into a detailed step-by-step guide, "
                    f"splitting it into logical pages or sections. Each page should be a complete unit of information:\n\n{text}"
                )
            }
        ],
        max_tokens=max_tokens,
        temperature=0.7
    )
    return response.choices[0].message.content

def process_links(links):
    current_folder = None

    for link_text, url in links:
        if "#" in url:
            current_folder = url.split("#")[-1].replace("#", "")
            folder_path = os.path.join(OUTPUT_DIR, current_folder)
            os.makedirs(folder_path, exist_ok=True)
            print(f"📁 Created folder: {folder_path}")
        elif url.endswith("/"):
            # Skip placeholder/root pages
            continue
        else:
            if not current_folder:
                print(f"⚠️ Skipping {url} (no folder context)")
                continue

            file_basename = sanitize_filename(get_last_path_segment(url))
            file_folder = os.path.join(OUTPUT_DIR, current_folder)
            os.makedirs(file_folder, exist_ok=True)
            
            #kunnen dit er later uit halen als we de html nietmeer willen
            try:
                html_response = requests.get(url)
                html_content = html_response.content.decode("utf-8")
                html_file = os.path.join(file_folder, file_basename + ".html")
                with open(html_file, "w", encoding="utf-8") as f:
                    f.write(html_content)
                print(f"🌐 Saved HTML: {html_file}")
            except Exception as e:
                print(f"⚠️ Failed to download HTML for {url}: {e}")
                continue

            text = scrape_page_to_text(url)

            if text:
                try:
                    print(f"🤖 Sending to GPT for: {link_text}")
                    gpt_response = process_with_gpt(text)
                    output_file = os.path.join(file_folder, file_basename + ".txt")
                    with open(output_file, "w", encoding="utf-8") as f:
                        f.write(f"{url}\n\n{gpt_response}")
                    print(f"📝 GPT response saved: {output_file}")
                except Exception as e:
                    print(f"❌ GPT processing failed for {url}: {e}")
            else:
                print(f"⚠️ Empty content for {url}")
