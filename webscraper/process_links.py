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
    PAGE_DELIMITER = "===PAGE_BREAK==="

    system_prompt = f"""
    You are a helpful assistant that provides detailed step-by-step guides. Format all content into a clean, professional, and consistent Markdown-based tutorial that follows these exact formatting rules:
    1. Use `#` for section titles (e.g. # Step-by-Step Guide to Remove the Part).
    2. Use `###` for main section headlines (e.g. ### Step-by-Step Guide to Remove the Spool Holder).
    3. Use `####` for each step (e.g. #### Step 1: Unscrew the panel).
    4. Use `1.`, `2.`, etc. for step-by-step instructions under each `####` heading.
    5. Use `**bold**` for emphasis, warnings, tool names, or critical notes.
    6. You MUST Split every section with a line containing only: {PAGE_DELIMITER}
    7. Use natural and structured formatting. Don't be verbose or conversational—be concise and technical.
    8. Do not add sections that aren't present in the original input.
    9. Do NOT use tables under any circumstance.
    10. Replace all tables with clear Markdown lists.
    - Convert parts, modules, and screws tables into bullet lists like:
        - **Part Name**
        - **Screw A** Location (X PCS)


    Output a complete and well-structured Markdown document following these rules.
    """.strip()

    user_prompt = f"""
    Please convert the following content into a structured Markdown-based guide using the formatting rules defined by the assistant:
    {text}
    """.strip()

    response = client.chat.completions.create(
        model=model,
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt}
        ],
        max_tokens=max_tokens,
        temperature=0.5
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
                    print(f"🤖 Sending to GPT")
                    gpt_response = process_with_gpt(text)
                    output_file = os.path.join(file_folder, file_basename + ".txt")
                    with open(output_file, "w", encoding="utf-8") as f:
                        f.write(f"{url}\n\n{gpt_response}")
                    print(f"📝 GPT response saved: {output_file}")
                except Exception as e:
                    print(f"❌ GPT processing failed for {url}: {e}")
            else:
                print(f"⚠️ Empty content for {url}")
