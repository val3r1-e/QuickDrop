from __future__ import annotations

import sys
import re
import time
from http.cookiejar import CookieJar
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import build_opener, HTTPCookieProcessor, Request


BASE_URL = sys.argv[1].rstrip("/") if len(sys.argv) > 1 else "http://127.0.0.1:5000"

jar = CookieJar()
opener = build_opener(HTTPCookieProcessor(jar))
failures: list[str] = []


def request(path: str, method: str = "GET", data: dict[str, str] | None = None) -> tuple[int, str, str]:
    if method == "POST" and data is not None:
        token_pages = {
            "/Account/Login": "/Account/Login",
            "/Account/Logout": "/Account/Login",
            "/Customer/PlaceOrder": "/Customer/Restaurant/1",
            "/Restaurant/CreateRestaurant": "/Restaurant/Manage",
            "/Restaurant/CreateProduct": "/Restaurant/Manage",
            "/Restaurant/UpdateStatus": "/Restaurant/Index",
            "/Driver/UpdateStatus": "/Driver/Index",
            "/Driver/Accept": "/Driver/Index",
        }
        token_page = token_pages.get(path)
        if token_page is not None:
            _, form_html, _ = request(token_page)
            match = re.search(
                r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',
                form_html,
            )
            if match is None:
                failures.append(f"{method} {path}: antiforgery token not found in {token_page}")
                return 0, "", ""
            data = {**data, "__RequestVerificationToken": match.group(1)}

    body = urlencode(data).encode() if data is not None else None
    req = Request(
        BASE_URL + path,
        data=body,
        method=method,
        headers={"Content-Type": "application/x-www-form-urlencoded"} if body else {},
    )
    try:
        with opener.open(req, timeout=10) as response:
            return response.status, response.read().decode("utf-8", errors="replace"), response.url
    except HTTPError as error:
        return error.code, error.read().decode("utf-8", errors="replace"), error.url
    except URLError as error:
        failures.append(f"{method} {path}: connection error: {error.reason}")
        return 0, "", ""


def check(name: str, condition: bool, detail: str = "") -> None:
    if condition:
        print(f"PASS: {name}")
    else:
        failures.append(name + (f": {detail}" if detail else ""))
        print(f"FAIL: {name}" + (f" - {detail}" if detail else ""))


def login(email: str, password: str) -> tuple[int, str, str]:
    return request("/Account/Login", "POST", {"email": email, "password": password})


public_routes = [
    ("home", "/", "Good Food"),
    ("home search", "/?q=Pizza", "Popular restaurants"),
    ("login page", "/Account/Login", "Welcome back"),
    ("register demo notice", "/Account/Register", "Registration is not enabled"),
    ("forgot demo notice", "/Account/Forgot", "Password recovery is not enabled"),
    ("customer restaurant", "/Customer/Restaurant/1", "Add"),
    ("track invalid order", "/Customer/Track/999999", None),
]

for name, path, expected in public_routes:
    status, html, final_url = request(path)
    if expected is None:
        check(name, status == 404, f"HTTP {status}")
    else:
        check(name, status == 200 and expected in html, f"HTTP {status}, final URL {final_url}")

check("stylesheet", request("/css/site.css")[0] == 200, "HTTP " + str(request("/css/site.css")[0]))
check("site script", request("/js/site.js")[0] == 200, "HTTP " + str(request("/js/site.js")[0]))

status, html, final_url = login("customer@gmail.com", "wrong-password")
check("invalid login", status == 200 and "Invalid email or password" in html, f"HTTP {status}")

status, html, final_url = login("customer@gmail.com", "Customer123!")
check("customer login redirect", status == 200 and "/Customer" in final_url, final_url)

status, html, final_url = request("/Customer/Index")
check("customer dashboard", status == 200 and "Customer Dashboard" in html, f"HTTP {status}")

status, html, final_url = request("/Customer/PlaceOrder", "POST", {
    "productId": "1",
    "quantity": "1",
    "deliveryAddress": "Automated test address",
    "paymentMethod": "Cash",
})
check("place order", status == 200 and "/Customer" in final_url, final_url)

latest_order_status, latest_order_html, _ = request("/Customer/Index")
order_ids = re.findall(r"/Customer/Track/(\d+)", latest_order_html)
test_order_id = order_ids[0] if order_ids else "1"
status, html, final_url = request(f"/Customer/Track/{test_order_id}")
check("track order", status == 200 and "Track" in html, f"HTTP {status}")

request("/Account/Logout", "POST", {})

status, html, final_url = login("manager@gmail.com", "Manager123!")
check("restaurant login redirect", status == 200 and "/Restaurant" in final_url, final_url)
check("restaurant dashboard", request("/Restaurant/Index")[0] == 200)
status, html, _ = request("/Restaurant/Manage")
check("restaurant management page", status == 200 and "Add a restaurant" in html, f"HTTP {status}")

restaurant_name = f"Automation Bistro {int(time.time())}"
product_name = f"Automation Special {int(time.time())}"
status, _, final_url = request("/Restaurant/CreateRestaurant", "POST", {
    "Name": restaurant_name,
    "Cuisine": "Test cuisine",
    "Address": "12 Test Street, Kampala",
    "Latitude": "0.3476",
    "Longitude": "32.5825",
    "ImageUrl": "https://example.com/restaurant.jpg",
    "ProductName": product_name,
    "ProductDescription": "Automated test dish",
    "ProductCategory": "Meals",
    "ProductPrice": "12500",
    "ProductImageUrl": "https://example.com/special.jpg",
})
check("create restaurant with initial menu item", status == 200 and "/Restaurant/Manage" in final_url, final_url)

status, html, _ = request("/?q=" + restaurant_name.replace(" ", "+"))
check("new restaurant appears in search", status == 200 and restaurant_name in html, f"HTTP {status}")
status, html, _ = request("/?q=" + product_name.replace(" ", "+"))
check("new menu item is searchable", status == 200 and product_name in html, f"HTTP {status}")

managed_status, managed_html, _ = request("/Restaurant/Manage")
restaurant_match = re.search(r'href="/Customer/Restaurant/(\d+)"', managed_html)
restaurant_id = restaurant_match.group(1) if restaurant_match else ""
check("restaurant location and price are listed", managed_status == 200 and "12 Test Street, Kampala" in managed_html and "12,500" in managed_html, f"HTTP {managed_status}")
if restaurant_id:
    status, _, final_url = request("/Restaurant/CreateProduct", "POST", {
        "RestaurantId": restaurant_id,
        "Name": "Second Test Dish",
        "Description": "Another automated menu item",
        "Category": "Meals",
        "Price": "9000",
        "ImageUrl": "https://example.com/second-dish.jpg",
    })
    check("add another menu item", status == 200 and "/Restaurant/Manage" in final_url, final_url)
else:
    check("add another menu item", False, "created restaurant link not found")

status, _, _ = request("/Restaurant/UpdateStatus", "POST", {"id": test_order_id, "status": "Preparing"})
check("restaurant status update", status == 200, f"HTTP {status}")

status, logout_html, final_url = request("/Account/Logout", "POST", {})
check("logout allows another person to sign in", status == 200 and "Login" in logout_html and final_url.endswith("/"), final_url)

status, html, final_url = login("driver@gmail.com", "Driver123!")
check("driver login redirect", status == 200 and "/Driver" in final_url, final_url)
check("driver dashboard", request("/Driver/Index")[0] == 200)

status, _, _ = request("/Driver/Accept", "POST", {"id": test_order_id})
check("driver accept order", status == 200, f"HTTP {status}")

status, _, _ = request("/Driver/UpdateStatus", "POST", {"id": test_order_id, "status": "On the Way"})
check("driver status update", status == 200, f"HTTP {status}")

request("/Account/Logout", "POST", {})

status, html, final_url = login("admin@gmail.com", "Admin123!")
check("admin login redirect", status == 200 and "/Admin" in final_url, final_url)
status, html, _ = request("/Admin/Index")
check("admin dashboard", status == 200 and "Admin Dashboard" in html, f"HTTP {status}")

for subsection in ("Customers", "Drivers", "Orders", "Payments", "Reports", "Settings"):
    status, _, final_url = request(f"/Admin/{subsection}")
    check(f"admin {subsection.lower()} route", status == 200 and "/Admin" in final_url, final_url)

print()
if failures:
    print(f"{len(failures)} test(s) failed:")
    for failure in failures:
        print(f"- {failure}")
    raise SystemExit(1)

print("All QuickDrop HTTP tests passed.")
